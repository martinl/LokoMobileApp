using loko.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace loko.Controls
{
    public class ExpanderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ScrolableExpander { get; set; }

        public DataTemplate SimpleExpander { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return (item as SettingsOptions).IsScrolable ? ScrolableExpander : SimpleExpander;
        }
    }
}
