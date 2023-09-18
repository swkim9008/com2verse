/*===============================================================
* Product:		Com2Verse
* File Name:	EmptyRenderer.cs
* Developer:	ljk
* Date:			2022-10-06 09:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Com2Verse.Rendering.RenderPipeline
{
	public sealed class EmptyRenderer : ScriptableRenderer
	{
		public override int SupportedCameraStackingTypes()
		{
			return 0;
		}
		
		private EmptyPass _empty;

		public EmptyRenderer(EmptyRendererData data) : base(data)
		{
			_empty = new EmptyPass();
		}

		public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			EnqueuePass(_empty);
		}
	}
	
    public class EmptyPass : ScriptableRenderPass
    {
	    private Color _clearColor = new Color(0, 0, 0, 0);
	    public EmptyPass()
	    {
	        base.profilingSampler = new ProfilingSampler(nameof(EmptyPass));
        }
	    
	    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	    {
		    cmd.ClearRenderTarget(true,true,_clearColor);
	    }

	    /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	    {   
	    }
    }
}
