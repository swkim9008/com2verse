/*===============================================================
* Product:		Com2Verse
* File Name:	ComparisonOperatorExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-13 18:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] ComparisonOperatorExtensions")]
	public sealed class ComparisonOperatorExtensions : Binder
	{
		public enum eOperationType
		{
			EQUALS,
			LESS_THAN,
			MORE_THAN,
		}

		public enum eType
		{
			BOOL,
			INT,
			STRING
		}


		[HideInInspector] [SerializeField] private eOperationType _operationType;
		[HideInInspector] [SerializeField] private eType _type;
		[HideInInspector] [SerializeField] private bool _boolean;
		[HideInInspector] [SerializeField] private int _int;
		[HideInInspector] [SerializeField] private string _string;

		private bool _isInitialized;
		private bool _result;

		[UsedImplicitly]
		public bool Boolean
		{
			get => _boolean;
			set
			{
				_result = value == _boolean;
				Sync();
			}
		}

		[UsedImplicitly]
		public int Int
		{
			get => _int;
			set
			{
				_result = value == _int;
				Sync();
			}
		}

		[UsedImplicitly]
		public string String
		{
			get => _string;
			set
			{
				_result = value == _string;
				Sync();
			}
		}

		public bool Result => _result;

		public override void Bind()
		{
			base.Bind();

			InitializeTarget();
		}


		private void Sync()
		{
			if (!_isInitialized)
			{
				InitializeTarget();
			}

			if (Result)
			{
				UpdateTargetProp();
			}
		}

		protected override void InitializeTarget()
		{
			SourceOwnerOfOneWayTarget = this;
			SourcePropertyInfoOfOneWayTarget = GetProperty(this.GetType(), nameof(Result));

			TargetOwnerOfOneWayTarget = _targetPath.component.IsReferenceNull() ? TargetViewModel : _targetPath.component;
			TargetPropertyInfoOfOneWayTarget = GetProperty(TargetOwnerOfOneWayTarget.GetType(), TargetPropertyName);

			_isInitialized = true;
		}
	}
}
