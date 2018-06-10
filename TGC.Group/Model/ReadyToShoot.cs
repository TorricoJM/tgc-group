﻿using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class ReadyToShoot : WeaponState
    {
        public ReadyToShoot(Weapon weapon) : base(weapon)
        {

        }

        public override void Move()
        {
            return;
        }

        public override void Update()
        {
            return;
        }

        public override void Shoot(Vehicle car)
        {
            this.weapon.weaponState = new Shooted(this.weapon, car);
            car.Remove(this.weapon);
        }

        public override void Render()
        {
            return;
        }

        override public TgcMesh GetCollidable(Vehicle car)
        {
            return null;
        }
    }
}