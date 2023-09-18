/*===============================================================
* Product:		Com2Verse
* File Name:	PacketBufferController.cs
* Developer:	haminjeong
* Date:			2023-01-09 12:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Buffers;
using System.Collections.Generic;

namespace Com2Verse.Network
{
    public class PacketBufferController
    {
        private const int PacketHeaderLengthForFirstElement = 1;
        private const byte PacketHeaderBitmaskForPacketLength = 0x0F;

        private const int MaxBufferSize = 1024 * 1024; // 다룰 수 있는 패킷의 최대 크기
        
        private byte[] _buffer           = new byte[MaxBufferSize];
        private byte[] _backgroundBuffer = new byte[MaxBufferSize];
        private int    _bufferReceivedOffset;
        private int    _bufferUsedOffset;
        private int    _bufferPacketStartOffset;

        private readonly Dictionary<int, byte[]> _headerCacheDic;
        private byte[] _header;
        private byte[] _body;
        private int _bodyLength;
        private int _bodyOffset;

        private int _lengthByte;
        
        private int _packetLength;


        private enum FilterActionResult
        {
            NotSet,
            Continue,
            Finished
        }
        
        private delegate FilterActionResult FilterAction();

        private event FilterAction _filterAction;

        public PacketBufferController()
        {
            _headerCacheDic = new();
            var key = LogicPacket.DataLength8BIT + PacketHeaderLengthForFirstElement;
            _headerCacheDic.Add(key, new byte[key]);
            key = LogicPacket.DataLength16BIT + PacketHeaderLengthForFirstElement;
            _headerCacheDic.Add(key, new byte[key]);
            key = LogicPacket.DataLength32BIT + PacketHeaderLengthForFirstElement;
            _headerCacheDic.Add(key, new byte[key]);
            ClearFilterBuffer();
            Reset();
        }

        private FilterActionResult SetHeaderByte()
        {
            if (PacketHeaderLengthForFirstElement > _bufferReceivedOffset - _bufferUsedOffset)
            {
                return FilterActionResult.NotSet;
            }

            _lengthByte = _buffer[_bufferUsedOffset] & PacketHeaderBitmaskForPacketLength;
            if (!_headerCacheDic.TryGetValue(PacketHeaderLengthForFirstElement + _lengthByte, out _header))
                throw new Exception("Unsupported header size");
            
            Buffer.BlockCopy(_buffer, _bufferUsedOffset, _header, 0, PacketHeaderLengthForFirstElement);
            _bufferUsedOffset += PacketHeaderLengthForFirstElement;
            
            _filterAction = SetDataLength;
            return FilterActionResult.Continue;
        }

        private FilterActionResult SetDataLength()
        {
            if (_lengthByte > _bufferReceivedOffset - _bufferUsedOffset)
            {
                return FilterActionResult.NotSet;
            }
            
            Buffer.BlockCopy(_buffer, _bufferUsedOffset, _header, PacketHeaderLengthForFirstElement, _lengthByte);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(_buffer, _bufferUsedOffset, _lengthByte);
            }
            ArraySegment<byte> PacketLength = new ArraySegment<byte>(_buffer, _bufferUsedOffset, _lengthByte);
            _packetLength = _lengthByte switch
            {
                LogicPacket.DataLength8BIT => PacketLength[0],
                LogicPacket.DataLength16BIT => BitConverter.ToUInt16(PacketLength),
                LogicPacket.DataLength32BIT => BitConverter.ToInt32(PacketLength),
                _ => throw new Exception("Unsupported value")
            };
            _bufferUsedOffset += _lengthByte;

            _bodyLength = _packetLength - (PacketHeaderLengthForFirstElement + _lengthByte);
            _body = ArrayPool<byte>.Shared.Rent(_bodyLength);

            _filterAction = MakeBody;
            return FilterActionResult.Continue;
        }
        
        private FilterActionResult MakeBody()
        {
            int bufferRemainLength = _bufferReceivedOffset - _bufferUsedOffset;
            int bodyRemainLength = _bodyLength - _bodyOffset; // 현재까지 처리한 패킷을 제외한 나머지 패킷의 길이
            int copyLength = bufferRemainLength < bodyRemainLength ? bufferRemainLength : bodyRemainLength;

            Buffer.BlockCopy(_buffer, _bufferUsedOffset, _body, _bodyOffset, copyLength);
            _bodyOffset += copyLength;
            _bufferUsedOffset += copyLength;
            
            if (_bodyLength > _bodyOffset)
            {
                return FilterActionResult.NotSet;
            }
            
            return FilterActionResult.Finished;
        }
        
        /// <summary>
        /// 분할된 패킷을 읽어들입니다.
        /// </summary>
        /// <param name="readBuffer">현재 도착한 패킷 버퍼</param>
        /// <param name="length">현재 도착한 패킷 길이</param>
        /// <param name="rest">현재 패킷의 남은 처리 바이트 수</param>
        /// <returns>처리 완료된 패킷의 결과가 byte[]로 반환되고, 그렇지 않으면 null을 반환합니다.</returns>
        public byte[] Filter(byte[] readBuffer, int length, ref int rest)
        {
            if (0 == rest)
            {
                if (length > _buffer.Length - _bufferReceivedOffset)
                {
                    SwapBuffer();
                }
                
                Buffer.BlockCopy(readBuffer, 0, _buffer, _bufferReceivedOffset, length);
                _bufferReceivedOffset += length;
            }

            FilterActionResult result;
            
            do
            {
                result = _filterAction.Invoke();
            } while (FilterActionResult.Continue == result);

            if (result == FilterActionResult.Finished)
            {
                rest = _bufferReceivedOffset - _bufferUsedOffset;
                var bodyData = new byte[_header.Length + _bodyLength];
                Buffer.BlockCopy(_header, 0, bodyData, 0, _header.Length);
                Buffer.BlockCopy(_body, 0, bodyData, _header.Length, _bodyLength);
                ArrayPool<byte>.Shared.Return(_body);

                if (0 == rest)
                {
                    ClearFilterBuffer();
                }
                Reset();
                
                return bodyData;
            }
            
            rest = 0;
            return null;
        }

        private void SwapBuffer()
        {
            int copyStartOffset             = _bufferPacketStartOffset == 0 ? _bufferUsedOffset : _bufferPacketStartOffset;
            int currentPacketReceivedLength = _bufferReceivedOffset - copyStartOffset;

            if (0 < currentPacketReceivedLength)
            {
                Buffer.BlockCopy(_buffer, copyStartOffset, _backgroundBuffer, 0, currentPacketReceivedLength);
            }

            (_buffer, _backgroundBuffer) = (_backgroundBuffer, _buffer);

            _bufferReceivedOffset    = currentPacketReceivedLength;
            _bufferUsedOffset        = _bufferUsedOffset - copyStartOffset;
            _bufferPacketStartOffset = 0;
        }

        private void ClearFilterBuffer()
        {
            _bufferReceivedOffset    = 0;
            _bufferUsedOffset        = 0;
            _bufferPacketStartOffset = 0;
        }

        // 1. 클라이언트가 처음 접속 했을 때 호출됨 (생성자를 통해)
        // 2. 한 패킷을 만들어 보낸 뒤 (Filter()함수 안)
        private void Reset()
        {
            _bufferPacketStartOffset = 0;
            
            _body = null;
            _bodyOffset = 0;

            _filterAction = SetHeaderByte;
        }
    }
}
