/*===============================================================
* Product:		Com2Verse
* File Name:	MiceKeyInfo.cs
* Developer:	seaman2000
* Date:			2023-05-15 12:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Com2Verse
{
    public struct MiceEventID : IComparable<MiceEventID>, IEquatable<MiceEventID>
    {
        public long value;

        public bool IsValid() => value != 0;
        public override int GetHashCode() => value.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is not MiceEventID ID) return false;
            return this.Equals(ID);
        }
        public bool Equals(MiceEventID obj) => obj.value == value;
        public int CompareTo(MiceEventID other) => this.value.CompareTo(other.value);

        public static implicit operator long(MiceEventID ID) => ID.value;
        public override string ToString() => value.ToString();
    }

    public static partial class MiceIDExtensions
    {
        public static MiceEventID ToMiceEventID(this long value) => new MiceEventID() { value = value };
    }
}


namespace Com2Verse
{
    public struct MiceProgramID : IComparable<MiceProgramID>, IEquatable<MiceProgramID>
    {
        public long value;

        public bool IsValid() => value != 0;
        public override int GetHashCode() => this.value.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is not MiceProgramID ID) return false;
            return this.Equals(ID);
        }
        public bool Equals(MiceProgramID obj) => obj.value == this.value;

        public int CompareTo(MiceProgramID other) => this.value.CompareTo(other.value);

        public static implicit operator long(MiceProgramID ID) => ID.value;
        public override string ToString() => value.ToString();
    }

    public static partial class MiceIDExtensions
    {
        public static MiceProgramID ToMiceProgramID(this long value) => new MiceProgramID() { value = value };
    }
}

namespace Com2Verse
{
    public struct MiceSessionID : IComparable<MiceSessionID>, IEquatable<MiceSessionID>
    {
        public long value;

        public bool IsValid() => value != 0;
        public override int GetHashCode() => this.value.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is not MiceSessionID ID) return false;
            return this.Equals(ID);
        }
        public bool Equals(MiceSessionID obj) => obj.value == this.value;
        public int CompareTo(MiceSessionID other) => this.value.CompareTo(other.value);

        public static implicit operator long(MiceSessionID ID) => ID.value;
        public override string ToString() => value.ToString();
    }

    public static partial class MiceIDExtensions
    {
        public static MiceSessionID ToMiceSessionID(this long value) => new MiceSessionID() { value = value };
    }
}