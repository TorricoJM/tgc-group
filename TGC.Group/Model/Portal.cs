﻿using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Group.Model.Vehiculos;
using TGC.Group.Lighting;

namespace TGC.Group.Model
{
    class Portal
    {
        private TGCVector3 position;
        public TgcMesh mesh;
        TGCMatrix transformation;
        float time = 1000;

        Lighting.Light light;

        private ColorValue EmissiveModifier = new ColorValue(50, 0, 0);
        private ColorValue AmbientModifier = new ColorValue(255, 255, 255);
        private ColorValue DiffuseModifier = new ColorValue(255, 150, 150);

        public Portal(TGCVector3 position, TGCMatrix transformationMatrix)
        {
            this.position = position;
            this.transformation = transformationMatrix;

            float intensity = 50;
            float attenuation = 80;
            this.light = new Lighting.Light(new ColorValue(255, 255, 255), position, intensity, attenuation);

        }

        public void CreateMesh(TgcMesh mesh)
        {
            this.mesh = mesh;
            this.mesh.AutoTransform = false;
            this.Transform();
            this.mesh.Effect = TgcShaders.loadEffect(GlobalConcepts.GetInstance().GetShadersDir() + "Portal.fx");
            this.mesh.Technique = "Portal";

        }

        public TGCVector3 GetPosition()
        {
            return this.position;
        }

        public void Dispose()
        {
            //no se por que rompe esta mierda
            //this.mesh.Dispose();
        }

        public void Render()
        {
            SetTime(time += GlobalConcepts.GetInstance().GetElapsedTime());
            mesh.Effect.SetValue("time", time);

            //this.Rotate(TGCMatrix.RotationZ(0.05f));
            this.Transform();
            this.mesh.Render();
        }

        public void Transform()
        {
            this.mesh.Transform = this.transformation;
            this.mesh.BoundingBox.transform(this.transformation);
        }

        public void Rotate(TGCMatrix Rotation)
        {
            this.transformation = Rotation * this.transformation;
        }

        public TgcBoundingAxisAlignBox GetBoundingBox()
        {
            this.Transform();
            return this.mesh.BoundingBox;
        }

        private void SetTime(float _time)
        {
            time = _time;
            if(time > 100000)
            {
                time = 1000;
            }
        }
    }
}
