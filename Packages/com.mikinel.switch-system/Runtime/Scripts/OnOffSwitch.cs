namespace mikinel.vrc.SwitchSystem
{
    /// <summary>
    /// State=0のときOff
    /// State=1のときOn
    /// と扱うスイッチ
    /// </summary>
    public class OnOffSwitch : SwitchBase
    {
        protected override void OnInteract()
        {
            SetState(localState == 0 ? 1 : 0);
        }
    }
}