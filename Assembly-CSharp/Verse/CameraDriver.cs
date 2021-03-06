using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class CameraDriver : MonoBehaviour
	{
		public CameraShaker shaker = new CameraShaker();

		private Camera cachedCamera;

		private GameObject reverbDummy;

		public CameraMapConfig config = new CameraMapConfig_Normal();

		private Vector3 velocity;

		private Vector3 rootPos;

		private float rootSize;

		private float desiredSize;

		private Vector2 desiredDolly = Vector2.zero;

		private Vector2 mouseDragVect = Vector2.zero;

		private bool mouseCoveredByUI;

		private float mouseTouchingScreenBottomEdgeStartTime = -1f;

		private static int lastViewRectGetFrame = -1;

		private static CellRect lastViewRect;

		public const float MaxDeltaTime = 0.025f;

		private const float ScreenDollyEdgeWidth = 20f;

		private const float ScreenDollyEdgeWidth_BottomFullscreen = 6f;

		private const float MinDurationForMouseToTouchScreenBottomEdgeToDolly = 0.28f;

		private const float MapEdgeClampMarginCells = -2f;

		public const float StartingSize = 24f;

		private const float MinSize = 11f;

		private const float MaxSize = 60f;

		private const float ZoomSpeed = 2.6f;

		private const float ZoomTightness = 0.4f;

		private const float ZoomScaleFromAltDenominator = 35f;

		private const float PageKeyZoomRate = 4f;

		private const float ScrollWheelZoomRate = 0.35f;

		public const float MinAltitude = 15f;

		private const float MaxAltitude = 65f;

		private const float ReverbDummyAltitude = 65f;

		private Camera MyCamera
		{
			get
			{
				if ((Object)this.cachedCamera == (Object)null)
				{
					this.cachedCamera = base.GetComponent<Camera>();
				}
				return this.cachedCamera;
			}
		}

		private float ScreenDollyEdgeWidthBottom
		{
			get
			{
				if (Screen.fullScreen)
				{
					return 6f;
				}
				return 20f;
			}
		}

		public CameraZoomRange CurrentZoom
		{
			get
			{
				if (this.rootSize < 12.0)
				{
					return CameraZoomRange.Closest;
				}
				if (this.rootSize < 13.800000190734863)
				{
					return CameraZoomRange.Close;
				}
				if (this.rootSize < 42.0)
				{
					return CameraZoomRange.Middle;
				}
				if (this.rootSize < 57.0)
				{
					return CameraZoomRange.Far;
				}
				return CameraZoomRange.Furthest;
			}
		}

		private Vector3 CurrentRealPosition
		{
			get
			{
				return this.MyCamera.transform.position;
			}
		}

		private bool AnythingPreventsCameraMotion
		{
			get
			{
				return Find.WindowStack.WindowsPreventCameraMotion || WorldRendererUtility.WorldRenderedNow;
			}
		}

		public IntVec3 MapPosition
		{
			get
			{
				IntVec3 result = this.CurrentRealPosition.ToIntVec3();
				result.y = 0;
				return result;
			}
		}

		public CellRect CurrentViewRect
		{
			get
			{
				if (Time.frameCount != CameraDriver.lastViewRectGetFrame)
				{
					CameraDriver.lastViewRect = default(CellRect);
					float num = (float)UI.screenWidth / (float)UI.screenHeight;
					Vector3 currentRealPosition = this.CurrentRealPosition;
					CameraDriver.lastViewRect.minX = Mathf.FloorToInt((float)(currentRealPosition.x - this.rootSize * num - 1.0));
					Vector3 currentRealPosition2 = this.CurrentRealPosition;
					CameraDriver.lastViewRect.maxX = Mathf.CeilToInt(currentRealPosition2.x + this.rootSize * num);
					Vector3 currentRealPosition3 = this.CurrentRealPosition;
					CameraDriver.lastViewRect.minZ = Mathf.FloorToInt((float)(currentRealPosition3.z - this.rootSize - 1.0));
					Vector3 currentRealPosition4 = this.CurrentRealPosition;
					CameraDriver.lastViewRect.maxZ = Mathf.CeilToInt(currentRealPosition4.z + this.rootSize);
					CameraDriver.lastViewRectGetFrame = Time.frameCount;
				}
				return CameraDriver.lastViewRect;
			}
		}

		public static float HitchReduceFactor
		{
			get
			{
				float result = 1f;
				if (Time.deltaTime > 0.02500000037252903)
				{
					result = (float)(0.02500000037252903 / Time.deltaTime);
				}
				return result;
			}
		}

		public float CellSizePixels
		{
			get
			{
				return (float)((float)UI.screenHeight / (this.rootSize * 2.0));
			}
		}

		public void Awake()
		{
			this.ResetSize();
			this.reverbDummy = GameObject.Find("ReverbZoneDummy");
			this.ApplyPositionToGameObject();
			this.MyCamera.farClipPlane = 71.5f;
		}

		public void OnPreRender()
		{
			if (LongEventHandler.ShouldWaitForEvent || Find.VisibleMap != null)
				;
		}

		public void OnPreCull()
		{
			if (!LongEventHandler.ShouldWaitForEvent && Find.VisibleMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				Find.VisibleMap.weatherManager.DrawAllWeather();
			}
		}

		public void OnGUI()
		{
			GUI.depth = 100;
			if (!LongEventHandler.ShouldWaitForEvent && Find.VisibleMap != null)
			{
				UnityGUIBugsFixer.OnGUI();
				this.mouseCoveredByUI = false;
				if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
				{
					this.mouseCoveredByUI = true;
				}
				if (!this.AnythingPreventsCameraMotion)
				{
					if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
					{
						this.mouseDragVect = Event.current.delta;
						Event.current.Use();
					}
					float num = 0f;
					if (Event.current.type == EventType.ScrollWheel)
					{
						float num2 = num;
						Vector2 delta = Event.current.delta;
						num = (float)(num2 - delta.y * 0.34999999403953552);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.TinyInteraction);
					}
					if (KeyBindingDefOf.MapZoomIn.KeyDownEvent)
					{
						num = (float)(num + 4.0);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
					}
					if (KeyBindingDefOf.MapZoomOut.KeyDownEvent)
					{
						num = (float)(num - 4.0);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
					}
					this.desiredSize -= (float)(num * 2.5999999046325684 * this.rootSize / 35.0);
					if (this.desiredSize < 11.0)
					{
						this.desiredSize = 11f;
					}
					if (this.desiredSize > 60.0)
					{
						this.desiredSize = 60f;
					}
					this.desiredDolly = Vector3.zero;
					if (KeyBindingDefOf.MapDollyLeft.IsDown)
					{
						this.desiredDolly.x = (float)(0.0 - this.config.dollyRateKeys);
					}
					if (KeyBindingDefOf.MapDollyRight.IsDown)
					{
						this.desiredDolly.x = this.config.dollyRateKeys;
					}
					if (KeyBindingDefOf.MapDollyUp.IsDown)
					{
						this.desiredDolly.y = this.config.dollyRateKeys;
					}
					if (KeyBindingDefOf.MapDollyDown.IsDown)
					{
						this.desiredDolly.y = (float)(0.0 - this.config.dollyRateKeys);
					}
					if (this.mouseDragVect != Vector2.zero)
					{
						this.mouseDragVect *= CameraDriver.HitchReduceFactor;
						this.mouseDragVect.x *= -1f;
						this.desiredDolly += this.mouseDragVect * this.config.dollyRateMouseDrag;
						this.mouseDragVect = Vector2.zero;
					}
				}
			}
		}

		public void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				if ((Object)Current.SubcameraDriver != (Object)null)
				{
					Current.SubcameraDriver.UpdatePositions(this.MyCamera);
				}
			}
			else if (Find.VisibleMap != null)
			{
				Vector2 lhs = this.CalculateCurInputDollyVect();
				if (lhs != Vector2.zero)
				{
					float d = (float)((this.rootSize - 11.0) / 49.0 * 0.699999988079071 + 0.30000001192092896);
					this.velocity = new Vector3(lhs.x, 0f, lhs.y) * d;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraDolly, KnowledgeAmount.FrameInteraction);
				}
				if (!this.AnythingPreventsCameraMotion)
				{
					float d2 = Time.deltaTime * CameraDriver.HitchReduceFactor;
					this.rootPos += this.velocity * d2 * this.config.moveSpeedScale;
					float x = this.rootPos.x;
					IntVec3 size = Find.VisibleMap.Size;
					if (x > (float)size.x + -2.0)
					{
						ref Vector3 val = ref this.rootPos;
						IntVec3 size2 = Find.VisibleMap.Size;
						val.x = (float)((float)size2.x + -2.0);
					}
					float z = this.rootPos.z;
					IntVec3 size3 = Find.VisibleMap.Size;
					if (z > (float)size3.z + -2.0)
					{
						ref Vector3 val2 = ref this.rootPos;
						IntVec3 size4 = Find.VisibleMap.Size;
						val2.z = (float)((float)size4.z + -2.0);
					}
					if (this.rootPos.x < 2.0)
					{
						this.rootPos.x = 2f;
					}
					if (this.rootPos.z < 2.0)
					{
						this.rootPos.z = 2f;
					}
				}
				if (this.velocity != Vector3.zero)
				{
					this.velocity *= this.config.camSpeedDecayFactor;
					if (this.velocity.magnitude < 0.10000000149011612)
					{
						this.velocity = Vector3.zero;
					}
				}
				float num = this.desiredSize - this.rootSize;
				this.rootSize += (float)(num * 0.40000000596046448);
				this.shaker.Update();
				this.ApplyPositionToGameObject();
				Current.SubcameraDriver.UpdatePositions(this.MyCamera);
				if (Find.VisibleMap != null)
				{
					RememberedCameraPos rememberedCameraPos = Find.VisibleMap.rememberedCameraPos;
					rememberedCameraPos.rootPos = this.rootPos;
					rememberedCameraPos.rootSize = this.rootSize;
				}
			}
		}

		private void ApplyPositionToGameObject()
		{
			this.rootPos.y = (float)(15.0 + (this.rootSize - 11.0) / 49.0 * 50.0);
			this.MyCamera.orthographicSize = this.rootSize;
			this.MyCamera.transform.position = this.rootPos + this.shaker.ShakeOffset;
			Vector3 position = base.transform.position;
			position.y = 65f;
			this.reverbDummy.transform.position = position;
		}

		private Vector2 CalculateCurInputDollyVect()
		{
			Vector2 vector = this.desiredDolly;
			bool flag = false;
			if ((UnityData.isEditor || Screen.fullScreen) && Prefs.EdgeScreenScroll && !this.mouseCoveredByUI)
			{
				Vector2 mousePositionOnUI = UI.MousePositionOnUI;
				Vector2 point = mousePositionOnUI;
				point.y = (float)UI.screenHeight - point.y;
				Rect rect = new Rect(0f, 0f, 200f, 200f);
				Rect rect2 = new Rect((float)(UI.screenWidth - 250), 0f, 255f, 255f);
				Rect rect3 = new Rect(0f, (float)(UI.screenHeight - 250), 225f, 255f);
				Rect rect4 = new Rect((float)(UI.screenWidth - 250), (float)(UI.screenHeight - 250), 255f, 255f);
				MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
				if (Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect && mainTabWindow_Inspect.RecentHeight > rect3.height)
				{
					rect3.yMin = (float)UI.screenHeight - mainTabWindow_Inspect.RecentHeight;
				}
				if (!rect.Contains(point) && !rect3.Contains(point) && !rect2.Contains(point) && !rect4.Contains(point))
				{
					Vector2 b = new Vector2(0f, 0f);
					if (mousePositionOnUI.x >= 0.0 && mousePositionOnUI.x < 20.0)
					{
						b.x -= this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.x <= (float)UI.screenWidth && mousePositionOnUI.x > (float)UI.screenWidth - 20.0)
					{
						b.x += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y <= (float)UI.screenHeight && mousePositionOnUI.y > (float)UI.screenHeight - 20.0)
					{
						b.y += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y >= 0.0 && mousePositionOnUI.y < this.ScreenDollyEdgeWidthBottom)
					{
						if (this.mouseTouchingScreenBottomEdgeStartTime < 0.0)
						{
							this.mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
						}
						if (Time.realtimeSinceStartup - this.mouseTouchingScreenBottomEdgeStartTime >= 0.2800000011920929)
						{
							b.y -= this.config.dollyRateScreenEdge;
						}
						flag = true;
					}
					vector += b;
				}
			}
			if (!flag)
			{
				this.mouseTouchingScreenBottomEdgeStartTime = -1f;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				vector *= 2.4f;
			}
			return vector;
		}

		public void Expose()
		{
			if (Scribe.EnterNode("cameraMap"))
			{
				try
				{
					Scribe_Values.Look<Vector3>(ref this.rootPos, "camRootPos", default(Vector3), false);
					Scribe_Values.Look<float>(ref this.desiredSize, "desiredSize", 0f, false);
					this.rootSize = this.desiredSize;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public void ResetSize()
		{
			this.desiredSize = 24f;
			this.rootSize = this.desiredSize;
		}

		public void JumpToVisibleMapLoc(IntVec3 cell)
		{
			this.JumpToVisibleMapLoc(cell.ToVector3Shifted());
		}

		public void JumpToVisibleMapLoc(Vector3 loc)
		{
			this.rootPos = new Vector3(loc.x, this.rootPos.y, loc.z);
		}

		public void SetRootPosAndSize(Vector3 rootPos, float rootSize)
		{
			this.rootPos = rootPos;
			this.rootSize = rootSize;
			this.desiredDolly = Vector2.zero;
			this.desiredSize = rootSize;
			LongEventHandler.ExecuteWhenFinished(this.ApplyPositionToGameObject);
		}
	}
}
