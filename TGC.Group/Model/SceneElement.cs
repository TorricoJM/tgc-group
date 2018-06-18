﻿using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Mathematica;
using TGC.Core.Collision;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;

namespace TGC.Group.Model
{
    class SceneElement : Collidable
    {
        protected List<TgcMesh> elements = new List<TgcMesh>();
        protected TGCMatrix transformacion;

        public SceneElement(List<TgcMesh> elementos, TGCMatrix transformacion)
        {
            foreach (TgcMesh mesh in elementos)
            {
                mesh.AutoTransform = false;
                this.elements.Add(mesh);
            }
            this.transformacion = transformacion;
        }

        public bool IsInto(TGCVector3 minPoint, TGCVector3 maxPoint)
        {
            this.Transform();
            foreach (TgcMesh element in this.elements)
            {
                if (this.AnyIsInside(element.BoundingBox.computeCorners(), minPoint, maxPoint))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AnyIsInside(TGCVector3[] points, TGCVector3 minPoint, TGCVector3 maxPoint)
        {
            foreach (TGCVector3 point in points)
            {
                if (GlobalConcepts.GetInstance().IsBetweenXZ(point, minPoint, maxPoint))
                {
                    return true;
                }

            }
            return false;
        }

        public bool IsColliding(Weapon weapon, out Collidable elementOut)
        {
            foreach (TgcMesh element in this.elements)
            {
                if(TgcCollisionUtils.testSphereAABB(weapon.sphere, element.BoundingBox))
                {
                    elementOut = this;
                    return true;
                }
            }
            elementOut = null;
            return false;
        }

        public TgcMesh GetCollidable(Vehicle car)
        {
            foreach (TgcMesh elemento in this.elements)
            {
                if(TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), elemento.BoundingBox))
                {
                    return elemento;
                }
            }

            return null;
        }

        public SceneElement(List<TgcMesh> elementos)
        {
            foreach (TgcMesh mesh in elementos)
            {
                mesh.AutoTransform = false;
                this.elements.Add(mesh);
            }
        }

        public TgcBoundingAxisAlignBox GetBoundingAlignBox()
        {
            return this.elements[0].BoundingBox;
        }

        public TGCVector3 GetPosition()
        {
            return TGCVector3.transform(new TGCVector3(0,0,0), this.transformacion);
        }

        public virtual void Transform()
        {
            foreach(TgcMesh elemento in this.elements)
            {
                elemento.Transform = this.transformacion;
                elemento.BoundingBox.transform(this.transformacion);
            }
        }
       

        public virtual void Render()
        {
            this.Transform();
            foreach (TgcMesh elemento in this.elements)
            {
                if (this.IsInView(elemento))
                {
                    Lighting.LightManager.GetInstance().DoLightMe(elemento);
                    elemento.Render();
                    elemento.BoundingBox.Render();
                }
            }
        }

        public void Dispose()
        {
            foreach (TgcMesh elemento in this.elements)
            {
                //elemento.Dispose();
            }
        }

        private TGCVector3 GenerateOutput(TGCVector3 vector)
        {
            if ((vector.X >= 0 && vector.Z >= 0) || (vector.X < 0 && vector.Z > 0))
            {
                return new TGCVector3(-vector.X, vector.Y, vector.Z);
            }
            else
            {
                return new TGCVector3(vector.X, vector.Y, vector.Z);
            }
        }

        public TGCVector3 GetPosition(TgcMesh element)
        {
            return (element.BoundingBox.PMax + element.BoundingBox.PMin) * 0.5f;
        }

        public bool IsInView(TgcMesh mesh)
        {
            this.Transform();
            return TgcCollisionUtils.classifyFrustumAABB(GlobalConcepts.GetInstance().GetFrustum(), mesh.BoundingBox) != 0;
        }

        private TGCPlane SelectPlane(List<TGCPlane> planes, TGCVector3 testPoint)
        {
            GlobalConcepts g = GlobalConcepts.GetInstance();
            planes.Sort((x,y) => g.IsInFrontOf(testPoint, x).CompareTo(g.IsInFrontOf(testPoint, y)));
            planes.Reverse();
            return planes[0];
        }

        private TGCPlane CreatePlane(TgcRay ray, TgcBoundingAxisAlignBox.Face[] faces, TGCVector3 testPoint)
        {
            float instante;
            TGCVector3 intersection;
            List<TGCPlane> candidatesPlanes = new List<TGCPlane>();
            int loop = 0;
            foreach (TgcBoundingAxisAlignBox.Face face in faces)
            {
                if (TgcCollisionUtils.intersectRayPlane(ray, face.Plane, out instante, out intersection))
                {
                    candidatesPlanes.Add(face.Plane);
                }
                loop++;
            }

            return this.SelectPlane(candidatesPlanes, testPoint);

        }

        private void Collide(TgcMesh elemento, Vehicle car)
        {
            //direccion a la que estoy yendo antes de chocar
            TGCVector3 directionOfCollision = car.GetDirectionOfCollision();
            TgcRay ray = new TgcRay();
            ray.Origin = car.GetLastPosition();
            ray.Direction = directionOfCollision;
            //interseco el rayo con el aabb, para conocer un punto del plano con el que colisione
            TgcBoundingAxisAlignBox.Face[] faces;
            faces = elemento.BoundingBox.computeFaces();
            TGCPlane plane = this.CreatePlane(ray, faces, car.GetLastPosition());
            TGCVector3 normal = GlobalConcepts.GetInstance().GetNormalPlane(plane);
            TGCVector3 output = normal + directionOfCollision * 2;
            float angle = car.SetDirection(output, normal);
            car.Crash(angle);

            while (TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), elemento.BoundingBox))
            {
                car.Translate(TGCMatrix.Translation(-directionOfCollision * 0.1f));
                car.Transform();
            }
        }

        private bool IsColliding(TgcMesh elemento, Vehicle car)
        {
            return TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), elemento.BoundingBox);
            
        }

        public void HandleCollisions(Vehicle car)
        {
            this.Transform();
            foreach (TgcMesh elemento in this.elements)
            {
                
                if (this.IsColliding(elemento, car)) {
                    this.Collide(elemento, car);
                    return;
                }
            }
        }
    }
}
