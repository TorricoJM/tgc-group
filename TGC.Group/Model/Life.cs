﻿using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Group.Model.Vehiculos;
using static TGC.Group.Model.Vehiculos.SoundsManager;

namespace TGC.Group.Model
{
    class Life : Collidable
    {
        private TGCMatrix transformation;
        private TgcMesh mesh;
        private bool visible = true;
        private Timer timer;
        private float time = 0;
        private SoundsManager sound;

        public Life(TGCMatrix matrix)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GlobalConcepts.GetInstance().GetMediaDir() + "MeshCreator\\Meshes\\Otros\\Vida\\Vida-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.AutoTransform = false;
            Transform();
            timer = new Timer();
            mesh.Effect = TgcShaders.loadEffect(GlobalConcepts.GetInstance().GetShadersDir() + "Vida.fx");
            mesh.Technique = "Normal";
            sound = new SoundsManager();
            sound.AddSound(GetPosition(), 50, 0, "Duke\\DameIAmGood.wav", "Vida", false);
            mesh.AutoTransform = false;
            transformation = matrix;
        }
        public void Init()
        {
            
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void HandleCollision(ThirdPersonCamera camera)
        {
            return;
        }

        public void SetTexture(float u, float v)
        {
            return;
        }

        public bool IsInto(TGCVector3 minPoint, TGCVector3 maxPoint)
        {
            return GlobalConcepts.GetInstance().IsBetweenXZ(this.GetPosition(), minPoint, maxPoint);
        }

        public TGCVector3 GetPosition()
        {
            return TGCVector3.transform(new TGCVector3(0, 0, 0), this.transformation);
        }

        public TGCPlane GetPlaneOfCollision(TgcRay ray, Vehicle car)
        {
            return TGCPlane.FromPointNormal(this.GetPosition(), TGCVector3.Up);
        }

        public TgcMesh GetCollidable(Vehicle car)
        {
            return this.mesh;
        }

        public void Dispose()
        {
            this.mesh.Dispose();
        }

        public void Render()
        {
            time += GlobalConcepts.GetInstance().GetElapsedTime();
            if (time > 100f) time = 0f;
            sound.GetSound("Vida").Position = GetPosition();
            CheckTime();
            transformation = TGCMatrix.RotationYawPitchRoll(GlobalConcepts.GetInstance().GetElapsedTime(), 0, 0) * transformation;
            Transform();
            mesh.Effect.SetValue("time", time);
            if (this.IsInView(this.mesh) && visible)
            {
                this.mesh.Render();
            }
        }

        private void CheckTime()
        {
            if (!visible) timer.acumularTiempo(GlobalConcepts.GetInstance().GetElapsedTime());
            if(timer.tiempoTranscurrido() > 60f)
            {
                visible = true;
                timer.resetear();
            }
        }

        public bool IsInView(TgcMesh mesh)
        {
            this.Transform();
            return TgcCollisionUtils.classifyFrustumAABB(GlobalConcepts.GetInstance().GetFrustum(), mesh.BoundingBox) != 0;
        }

        public void Transform()
        {
            mesh.Transform = transformation;
            mesh.BoundingBox.transform(transformation);
        }

        public bool IsColliding(Vehicle car)
        {
            if (!visible) return false;
            Transform();
            return TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), mesh.BoundingBox);
        }

        public bool IsColliding(Weapon weapon)
        {
            return false;
        }

        public void HandleCollision(Weapon weapon)
        {
            return;
        }

        public void HandleCollision(Vehicle car)
        {
            if (IsColliding(car))
            {
                car.Cure(30);
                sound.GetSound("Vida").play();
                visible = false;
            }
            
        }

    }
}
