using UnityEngine;

[CreateAssetMenu(menuName = "Lantern Keeper/Lantern Flow Text", fileName = "LanternFlowText")]
public sealed class LanternFlowText : ScriptableObject
{
    public string pickupPrompt = "Press T";
    public string raiseHint = "Press E";
    public string brokenLadderHint = "Broken ladder";
    public string backToMenu = "Back to Menu";
    public string quit = "Quit";
}
