using StardewModdingAPI;

namespace WorldAtlas.Compatibility.GMCM
{
    public static class GMCMExtras
    {
        public static void AddWideTextOption(IManifest mod, Func<string> name, Func<string> getValue, Action<string> setValue, Func<string>? tooltip = null, int width = 400)
        {
            var gmcm = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null) return;

            WideTextBox option = new(width, 48);

            gmcm.AddComplexOption(
                mod: mod,
                name: name,
                draw: option.Draw,
                height: () => option.Height,
                tooltip: tooltip,
                beforeMenuOpened: () => option.Text = getValue(),
                beforeReset: () => option.Text = getValue(),
                beforeSave: () => {
                    if (option.Text == getValue()) return;
                    setValue(option.Text);
                }
            );


        }
    }
}
