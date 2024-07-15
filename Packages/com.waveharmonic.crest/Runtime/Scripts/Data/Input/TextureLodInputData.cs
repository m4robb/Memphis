// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using WaveHarmonic.Crest.Internal;

namespace WaveHarmonic.Crest
{
    [System.Serializable]
    abstract class TextureLodInputData : LodInputData
    {
        [Tooltip("Texture containing the data. This will be a similar format to painted data.")]
        [@DecoratedField, SerializeField]
        internal Texture _Texture;

        public override IPropertyWrapper GetProperties(CommandBuffer buffer) => new PropertyWrapperCompute(buffer, TextureShader, 0);
        protected abstract ComputeShader TextureShader { get; }
        protected internal override bool IsEnabled => _Texture != null;
        public override Vector2 HeightRange => throw new System.NotImplementedException();
        public override Rect Rect => _Input.transform.RectXZ();

        protected internal override void Draw(Lod lod, Component component, CommandBuffer buffer, RenderTexture target, int slices)
        {
            var transform = component.transform;
            var wrapper = new PropertyWrapperCompute(buffer, TextureShader, 0);
            var rotation = new Vector2(transform.localToWorldMatrix.m20, transform.localToWorldMatrix.m00).normalized;
            wrapper.SetVector(ShaderIDs.s_TextureSize, transform.lossyScale.XZ());
            wrapper.SetVector(ShaderIDs.s_TexturePosition, transform.position.XZ());
            wrapper.SetVector(ShaderIDs.s_TextureRotation, rotation);
            wrapper.SetVector(ShaderIDs.s_Resolution, new(_Texture.width, _Texture.height));
            wrapper.SetFloat(ShaderIDs.s_FeatherWidth, _Input.FeatherWidth);
            wrapper.SetTexture(ShaderIDs.s_Texture, _Texture);
            wrapper.SetInteger(ShaderIDs.s_Blend, (int)_Input.Blend);
            wrapper.SetTexture(ShaderIDs.s_Target, target);

            if (this is LevelTextureLodInputData height)
            {
                wrapper.SetKeyword(WaterResources.Instance.Keywords.LevelTextureCatmullRom, height._UseCatmullRomFiltering);
            }

            if (this is DirectionalTextureLodInputData data)
            {
                wrapper.SetBoolean(ShaderIDs.s_NegativeValues, data._NegativeValues);
            }

            var threads = lod.Resolution / Lod.k_ThreadGroupSize;
            wrapper.Dispatch(threads, threads, slices);
        }

        protected internal override void OnEnable()
        {
            // Empty.
        }

        protected internal override void OnDisable()
        {
            // Empty.
        }

        protected internal override void OnUpdate()
        {
            // Empty.
        }

#if UNITY_EDITOR
        protected internal override void OnChange(string propertyPath, object previousValue)
        {

        }

        public override bool InferMode(Component component, ref LodInputMode mode)
        {
            return false;
        }
#endif
    }

    [System.Serializable]
    abstract class DirectionalTextureLodInputData : TextureLodInputData
    {
        [@DecoratedField, SerializeField]
        internal bool _NegativeValues;
    }

    partial class LevelTextureLodInputData
    {
        [Label("Filtering (High Quality)")]
        [Tooltip("Helps with staircase aliasing.")]
        [@DecoratedField, SerializeField]
        internal bool _UseCatmullRomFiltering;
    }
}
