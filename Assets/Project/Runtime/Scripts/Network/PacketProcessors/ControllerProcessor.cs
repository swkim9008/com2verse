/*===============================================================
* Product:    Com2Verse
* File Name:  ControllerProcessor.cs
* Developer:  haminjeong
* Date:       2022-05-10 22:08
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.Controller)]
    public sealed class ControllerProcessor : BaseMessageProcessor
    {
        private static bool _isDebug = true;//AppInfo.Instance.Data.DistributeType.ToLower().Equals("debug");
        private static readonly string LatencyPath = "AppInfo/Canvas/Latency/Text";
        private bool _isTextInit = false;
        private TMP_Text _latencyText;
        
        public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.Controller.MessageTypes.LatencyCheck, 
                                      (payload) => Protocols.Controller.LatencyCheck.Parser.ParseFrom(payload),
                                      (message) =>
                                      {
                                          var latencyCheck = message as Protocols.Controller.LatencyCheck;
                                          long latency = latencyCheck.Time - MetaverseWatch.Time;
                                          if (_isDebug)
                                          {
                                              if (_latencyText.IsReferenceNull() && !_isTextInit)
                                              {
                                                  _isTextInit = true;
                                                  var latencyObject = GameObject.Find(LatencyPath);
                                                  if (!latencyObject.IsReferenceNull())
                                                      _latencyText = latencyObject.GetComponent<TMP_Text>();
                                              }
                                              if (!_latencyText.IsReferenceNull())
                                                  _latencyText.text = latency.ToString();
                                          }
                                          if (latencyCheck.Serial != 0 && latency > 100)
                                          {
                                              C2VDebug.Log($"Received latency check {latencyCheck.Serial} at {MetaverseWatch.Time}. Latency: {latency}");
                                              int rtt = NetworkManager.Instance.GetRtt();
                                              ServerTime.SetServerTime(latencyCheck.Time, rtt);
                                          }
                                      });
        }
    }
}
