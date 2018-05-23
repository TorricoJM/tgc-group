﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Collision;

namespace TGC.Group.Model
{
    class BidirectionalPortal : TypeOfPortal
    {
        private Portal targetPortal;

        public BidirectionalPortal(Portal originPortal, Portal targetPortal, TGCVector3 outDirection) : base(originPortal, outDirection)
        {
            this.targetPortal = targetPortal;
        }

        override public TGCVector3 GetTargetPosition()
        {
            return targetPortal.GetPosition();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.targetPortal.Dispose();
        }

        public override void Collide(Vehiculo car)
        {
            TGCVector3 newPosition = this.targetPortal.GetPosition();
            TGCMatrix translation = TGCMatrix.Translation(0,0,0);
            car.ChangePosition(TGCMatrix.Translation(newPosition.X, newPosition.Y, newPosition.Z));
            while (TgcCollisionUtils.testObbAABB(car.GetTGCBoundingOrientedBox(), this.targetPortal.GetBoundingBox()))
            {
                translation *= TGCMatrix.Translation(this.outDirection * 0.1f);
                car.ChangePosition(TGCMatrix.Translation(newPosition) * translation);
            }
            return;
        }

    }
}
