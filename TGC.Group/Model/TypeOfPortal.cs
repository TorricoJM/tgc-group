﻿using TGC.Core.Mathematica;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Vehiculos;
using System.Drawing;
using System;

namespace TGC.Group.Model
{
    abstract class TypeOfPortal : Collidable
    {
        protected Portal originPortal;
        protected TGCVector3 outDirection;
        protected SoundsManager soundManager;

        public TypeOfPortal(Portal originPortal, TGCVector3 outDirection)
        {
            this.outDirection = outDirection;
            this.originPortal = originPortal;
            this.soundManager = new SoundsManager();
            this.soundManager.AddSound(this.GetPosition(), 10f, 0, "Portal\\Goku.wav", "Goku", false);
        }

        public bool IsInView()
        {
            TGCPlane plane = Scene.GetInstance().camera.GetPlane();
            return GlobalConcepts.GetInstance().IsInFrontOf(this.GetPosition(), plane) || TgcCollisionUtils.testPlaneAABB(plane, this.originPortal.GetBoundingBox());
        }

        public TgcMesh GetCollidable(Vehicle car)
        {
            //return originPortal.mesh;
            return null;
        }

        public bool IsColliding(Weapon weapon, out Collidable element)
        {
            if(TgcCollisionUtils.testSphereAABB(weapon.sphere, this.GetBoundingAlignBox()))
            {
                element = this;
                return true;
            }
            element = null;
            return false;
        }

        public TgcBoundingAxisAlignBox GetBoundingAlignBox()
        {
            return this.originPortal.mesh.BoundingBox;
        }

        abstract public TGCVector3 GetTargetPosition();

        virtual public void Dispose()
        {
            this.originPortal.Dispose();
        }

        public void Render()
        {
            this.originPortal.Render();
        }

        public TgcBoundingAxisAlignBox GetBoundingBox()
        {
            return this.originPortal.GetBoundingBox();
        }

        public void HandleCollisions(Vehicle car)
        {
            if (TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), this.GetBoundingBox())){
                this.GetBoundingBox().setRenderColor(Color.Red);
                this.Collide(car);
                this.soundManager.GetSound("Goku").play();
            }
        }

        virtual public void Collide(Vehicle car)
        {
            var dot = TGCVector3.Dot(car.vectorAdelante, this.outDirection);
            var modulusProduct = car.vectorAdelante.Length() * this.outDirection.Length();
            var acos = (float)Math.Acos(dot / modulusProduct);
            var yCross = TGCVector3.Cross(car.vectorAdelante, this.outDirection).Y;
            var giro = (yCross > 0) ? acos : -acos;
            car.Girar(giro);
            Scene.GetInstance().camera.rotateY(giro);
            if (car.GetVelocidadActual() < 0)
            {
                car.Girar(FastMath.PI);
                Scene.GetInstance().camera.rotateY(FastMath.PI);
            }
        }

        public TGCVector3 GetPosition()
        {
            return this.originPortal.GetPosition();
        }
    }
}
