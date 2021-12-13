﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Hover
{
    public partial class ProjectileSimulator
    {
        public List<BulletDropProjectile> List { get; private set; }
        public Entity Owner { get; private set; }

        public ProjectileSimulator( Entity owner )
        {
            List = new();
            Owner = owner;
        }

        public void Add( BulletDropProjectile projectile )
        {
            List.Add( projectile );
        }

        public void Remove( BulletDropProjectile projectile )
        {
            List.Remove( projectile );
        }

        public void Clear()
        {
            foreach ( var projectile in List )
            {
                projectile.Delete();
            }

            List.Clear();
        }

        public void Simulate()
        {
            for ( int i = List.Count - 1; i >= 0; i-- )
            {
                var projectile = List[i];

                if ( !projectile.IsValid() )
                {
                    List.RemoveAt( i );
                    continue;
                }

                if ( Prediction.FirstTime )
                    projectile.Simulate();
            }
        }
    }

    public static class ProjectileSimulatorExtensions
    {
        public static bool IsValid( this ProjectileSimulator simulator )
        {
            return simulator != null && ( simulator.Owner?.IsValid() ?? false );
        }
    }
}
