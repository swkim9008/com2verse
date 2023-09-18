/*===============================================================
* Product:		Com2Verse
* File Name:	UICameraRenderer.cs
* Developer:	ljk
* Date:			2022-08-22 09:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Com2Verse.Rendering.RenderPipeline
{
	public sealed class UICameraRenderer : ScriptableRenderer
	{
		public override int SupportedCameraStackingTypes()
		{
			return 0;
		}
		
		private RenderTargetBufferSystem _colorBuffer;
#if UNITY_2021
		private RenderTargetHandle _activeCameraColorAttachment;
#elif UNITY_2022
		private RTHandle _activeCameraColorAttachment;
#endif
		private DrawObjectsPass _renderTransparentForwardPass;

		public UICameraRenderer(UICameraRendererData data) : base(data)
		{
			StencilStateData stencilData = data._defaultStencilState;
			
			var defaultStencilState = StencilState.defaultValue;
			defaultStencilState.enabled = stencilData.overrideStencilState;
			defaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
			defaultStencilState.SetPassOperation(stencilData.passOperation);
			defaultStencilState.SetFailOperation(stencilData.failOperation);
			defaultStencilState.SetZFailOperation(stencilData.zFailOperation);
			
			_renderTransparentForwardPass = new DrawObjectsPass(URPProfileId.DrawTransparentObjects, 
				false, 
				RenderPassEvent.BeforeRenderingTransparents, 
				RenderQueueRange.transparent, 
				data.TransparentLayerMask,
				defaultStencilState, 
				stencilData.stencilReference);

			_colorBuffer = new RenderTargetBufferSystem("_CameraColorAttachment");
		}

		public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			ref CameraData cameraData = ref renderingData.cameraData;

#if UNITY_2021
			_activeCameraColorAttachment = RenderTargetHandle.GetCameraTarget(cameraData.xr);
#elif UNITY_2022
			_activeCameraColorAttachment = cameraData.renderer.cameraColorTargetHandle;
#endif

			CreateCameraRenderTarget(context,ref cameraData.cameraTargetDescriptor);
			
#if UNITY_2021
			ConfigureCameraColorTarget(_activeCameraColorAttachment.Identifier()); // 컬러만 설정
#elif UNITY_2022
			ConfigureCameraColorTarget(_activeCameraColorAttachment);
#endif

			RenderBufferStoreAction transparentPassColorStoreAction = RenderBufferStoreAction.Resolve;// RenderBufferStoreAction.Store;
			RenderBufferStoreAction transparentPassDepthStoreAction = RenderBufferStoreAction.DontCare;

			_renderTransparentForwardPass.ConfigureColorStoreAction(transparentPassColorStoreAction);
			_renderTransparentForwardPass.ConfigureDepthStoreAction(transparentPassDepthStoreAction);
			EnqueuePass(_renderTransparentForwardPass);
		}

		public override void FinishRendering(CommandBuffer cmd)
		{
#if UNITY_2021
			_colorBuffer.Clear(cmd);
			if(_activeCameraColorAttachment != RenderTargetHandle.CameraTarget)
				_activeCameraColorAttachment = RenderTargetHandle.CameraTarget;
#elif UNITY_2022
			_colorBuffer.Clear();
			if(_colorBuffer.PeekBackBuffer() is { } colorTargetBuffer && _activeCameraColorAttachment != colorTargetBuffer)
				_activeCameraColorAttachment = colorTargetBuffer;
#endif
		}

		void CreateCameraRenderTarget(ScriptableRenderContext context, ref RenderTextureDescriptor descriptor)
		{
			CommandBuffer cmd = CommandBufferPool.Get();

#if UNITY_2021
			if (_activeCameraColorAttachment != RenderTargetHandle.CameraTarget)
#elif UNITY_2022
			if (_activeCameraColorAttachment != _colorBuffer.PeekBackBuffer())
#endif
			{
				RenderTextureDescriptor colorDescriptor = descriptor;
				colorDescriptor.useMipMap = false;
				colorDescriptor.autoGenerateMips = false;
				colorDescriptor.depthBufferBits = 0;
#if UNITY_2021
				_colorBuffer.SetCameraSettings(cmd,colorDescriptor, FilterMode.Bilinear);
				ConfigureCameraColorTarget(_colorBuffer.GetBackBuffer(cmd).id);
				_activeCameraColorAttachment = _colorBuffer.GetBackBuffer();
				cmd.SetGlobalTexture("_CameraColorTexture", _activeCameraColorAttachment.id);
				cmd.SetGlobalTexture("_AfterPostProcessTexture", _activeCameraColorAttachment.id);
#elif UNITY_2022
				_colorBuffer.SetCameraSettings(colorDescriptor, FilterMode.Bilinear);
				ConfigureCameraColorTarget(_colorBuffer.GetBackBuffer(cmd));
				_activeCameraColorAttachment = _colorBuffer.GetBackBuffer(cmd);
				cmd.SetGlobalTexture("_CameraColorTexture", _activeCameraColorAttachment);
				cmd.SetGlobalTexture("_AfterPostProcessTexture", _activeCameraColorAttachment);
#endif
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
