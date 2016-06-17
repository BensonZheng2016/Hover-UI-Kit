using System;
using Hover.Cursors;
using UnityEngine;

namespace Hover.Items.Managers {

	/*================================================================================================*/
	[RequireComponent(typeof(HoverItem))]
	[RequireComponent(typeof(HoverItemHighlightState))]
	public class HoverItemSelectionState : MonoBehaviour {

		public float SelectionProgress { get; private set; }
		public bool IsSelectionPrevented { get; private set; }
		
		private DateTime? vSelectionStart;
		private float vDistanceUponSelection;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Update() {
			TryResetSelection();
			UpdateSelectionProgress();
			UpdateState();
			UpdateNearestCursor();
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void TryResetSelection() {
			if ( !GetComponent<HoverItemHighlightState>().IsHighlightPrevented ) {
				return;
			}
			
			HoverItemData itemData = GetComponent<HoverItem>().Data;
			ISelectableItem selData = (itemData as ISelectableItem);
			
			vSelectionStart = null;
			
			if ( selData != null ) {
				selData.DeselectStickySelections();
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateSelectionProgress() {
			HoverItemHighlightState highState = GetComponent<HoverItemHighlightState>();

			if ( vSelectionStart == null ) {
				HoverItemData itemData = GetComponent<HoverItem>().Data;
				ISelectableItem selData = (itemData as ISelectableItem);

				if ( selData == null || !selData.IsStickySelected ) {
					SelectionProgress = 0;
					return;
				}
					
				HoverItemHighlightState.Highlight? nearestHigh = highState.NearestHighlight;
				float nearDist = highState.InteractionSettings.StickyReleaseDistance;
				float minHighDist = (nearestHigh == null ? float.MaxValue : nearestHigh.Value.Distance);

				SelectionProgress = Mathf.InverseLerp(nearDist, vDistanceUponSelection, minHighDist);
				return;
			}
				
			float ms = (float)(DateTime.UtcNow-(DateTime)vSelectionStart).TotalMilliseconds;
			SelectionProgress = Math.Min(1, ms/highState.InteractionSettings.SelectionMilliseconds);
		}

		/*--------------------------------------------------------------------------------------------*/
		private bool UpdateState() {
			HoverItemData itemData = GetComponent<HoverItem>().Data;
			ISelectableItem selData = (itemData as ISelectableItem);

			if ( selData == null ) {
				return false;
			}

			////

			HoverItemHighlightState highState = GetComponent<HoverItemHighlightState>();
			
			bool canSelect = (
				!highState.IsHighlightPrevented && 
				highState.IsNearestAcrossAllItemsForAnyCursor &&
				selData.AllowSelection
			);
			
			if ( SelectionProgress <= 0 || !canSelect ) {
				selData.DeselectStickySelections();
			}

			if ( !canSelect ) {
				IsSelectionPrevented = false;
				vSelectionStart = null;
				return false;
			}

			////

			HoverItemHighlightState.Highlight? nearestHigh = 
				GetComponent<HoverItemHighlightState>().NearestHighlight;
			
			if ( nearestHigh == null || nearestHigh.Value.Progress < 1 ) {
				IsSelectionPrevented = false;
				vSelectionStart = null;
				return false;
			}

			////

			if ( IsSelectionPrevented ) {
				vSelectionStart = null;
				return false;
			}

			if ( vSelectionStart == null ) {
				vSelectionStart = DateTime.UtcNow;
				return false;
			}

			if ( SelectionProgress < 1 ) {
				return false;
			}

			vSelectionStart = null;
			IsSelectionPrevented = true;
			vDistanceUponSelection = nearestHigh.Value.Distance;
			selData.Select();
			return true;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateNearestCursor() {
			HoverItemHighlightState highState = GetComponent<HoverItemHighlightState>();
			HoverItemHighlightState.Highlight? nearestHigh = highState.NearestHighlight;

			if ( nearestHigh == null ) {
				return;
			}

			IHoverCursorData cursor = nearestHigh.Value.Cursor;

			cursor.MaxItemSelectionProgress = Mathf.Max(
				cursor.MaxItemSelectionProgress, SelectionProgress);
		}

	}

}
