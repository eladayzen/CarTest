// Description: CombatThemeDefinition. ScriptableObject naming a set of AbilityDefinitions for
// one Combat Run "build" (e.g. TMNT vs a later General theme). CombatRunManager references
// exactly one theme asset - that's the single swap point for a different theme later; the
// prototype just assigns one in the Inspector. Create instances via
// Assets > Create > Combat Run > Theme Definition.
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    [CreateAssetMenu(fileName = "NewTheme", menuName = "Combat Run/Theme Definition")]
    public class CombatThemeDefinition : ScriptableObject
    {
        public string                          themeName = "New Theme";
        public List<AbilityDefinition>         abilities = new List<AbilityDefinition>();
    }
}
