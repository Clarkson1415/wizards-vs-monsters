using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardsVsMonster.scripts
{
    public class CalculationUtils
    {
        public static Area2D GetAreaAtPoint(Vector2 globalPos, Control someNodeToUseWorld)
        {
            var spaceState = someNodeToUseWorld.GetWorld2D().DirectSpaceState;

            var query = new PhysicsPointQueryParameters2D
            {
                Position = globalPos,
                CollideWithBodies = false,
                CollideWithAreas = true
            };

            query.CollideWithBodies = false;
            query.CollideWithAreas = true;

            var results = spaceState.IntersectPoint(query);

            foreach (var result in results)
            {
                var colliderObj = result["collider"];
                if (colliderObj.VariantType == Variant.Type.Object)
                {
                    var area = colliderObj.As<Area2D>();
                    if (area != null)
                    {
                        GD.Print($"Clicked on area: {area.Name}");
                        return area;
                    }
                }
            }

            return null;
        }
    }
}
