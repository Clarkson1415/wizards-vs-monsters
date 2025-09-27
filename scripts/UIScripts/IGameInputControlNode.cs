using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WizardsVsMonster.scripts.UIScripts
{
    public interface IGameInputControlNode
    {
        public abstract void InputTap(Vector2 tapPosition);
    }
}
