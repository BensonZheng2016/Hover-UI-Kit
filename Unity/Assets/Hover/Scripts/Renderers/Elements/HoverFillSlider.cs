using System;
using System.Collections.Generic;
using Hover.Renderers.Utils;
using Hover.Utils;
using UnityEngine;

namespace Hover.Renderers.Elements {

	/*================================================================================================*/
	[RequireComponent(typeof(HoverIndicator))]
	[RequireComponent(typeof(HoverShape))]
	public class HoverFillSlider : HoverFill {

		public const string SegmentInfoName = "SegmentInfo";
		public const int SegmentCount = 4;

		[DisableWhenControlled(DisplaySpecials=true)]
		public HoverRendererSliderSegments SegmentInfo;
		
		[DisableWhenControlled]
		public GameObject TickPrefab;

		[DisableWhenControlled]
		public HoverMesh SegmentA;

		[DisableWhenControlled]
		public HoverMesh SegmentB;

		[DisableWhenControlled]
		public HoverMesh SegmentC;

		[DisableWhenControlled]
		public HoverMesh SegmentD;
		
		[DisableWhenControlled]
		public List<HoverMesh> Ticks;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public override int GetChildMeshCount() {
			return SegmentCount;
		}

		/*--------------------------------------------------------------------------------------------*/
		public override HoverMesh GetChildMesh(int pIndex) {
			switch ( pIndex ) {
				case 0: return SegmentA;
				case 1: return SegmentB;
				case 2: return SegmentC;
				case 3: return SegmentD;
			}

			throw new ArgumentOutOfRangeException();
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public override void TreeUpdate() {
			base.TreeUpdate();
			UpdateTickList();
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateTickList() {
			int newTickCount = SegmentInfo.TickInfoList.Count;

#if UNITY_EDITOR
			//ticks are often added within a prefab; this forces serialization of the "Ticks" list
			if ( Ticks.Count != newTickCount ) {
				UnityEditor.EditorUtility.SetDirty(this);
			}
#endif

			if ( TickPrefab == null ) {
				Debug.LogWarning("Cannot build ticks without a prefab reference.", this);
				return;
			}

			while ( Ticks.Count < newTickCount ) {
				HoverMesh tickMesh = RendererUtil.TryBuildPrefabRenderer<HoverMesh>(TickPrefab);
				tickMesh.name = "Tick"+Ticks.Count;
				tickMesh.transform.SetParent(gameObject.transform, false);
				Ticks.Add(tickMesh);
			}

			while ( Ticks.Count > newTickCount ) {
				int lastTickIndex = Ticks.Count-1;
				HoverMesh tick = Ticks[lastTickIndex];

				Ticks.RemoveAt(lastTickIndex);

				if ( Application.isPlaying ) {
					Destroy(tick.gameObject);
				}
				else {
					DestroyImmediate(tick.gameObject);
				}
			}
		}

	}

}