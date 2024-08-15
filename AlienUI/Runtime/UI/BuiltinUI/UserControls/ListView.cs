using AlienUI.Core.Commnands;
using AlienUI.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AlienUI.UIElements
{
    public class ListView : UserControl
    {
        public override ControlTemplate DefaultTemplate => new ControlTemplate("Builtin.ListView");
        
        public IList Items
        {
            get { return (IList)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IList), typeof(ListView), new PropertyMetadata(new List<object>()), OnItemsChanged);

        private static void OnItemsChanged(DependencyObject sender, object oldValue, object newValue)
        {
            var self = sender as ListView;
        }

        public ItemTemplate ItemTemplate
        {
            get { return (ItemTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(ItemTemplate), typeof(ListView), new PropertyMetadata(new ItemTemplate("Builtin.SimpleItem")));

        public Command<UIElement> SelectItemCmd
        {
            get { return (Command<UIElement>)GetValue(SelectItemCmdProperty); }
            set { SetValue(SelectItemCmdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectItemCmdProperty =
            DependencyProperty.Register("SelectItemCmdProperty", typeof(Command<UIElement>), typeof(ListView), new PropertyMetadata(null));

        protected override void OnInitialized()
        {
            SelectItemCmd = new Command<UIElement>();
            SelectItemCmd.OnExecute += SelectItemCmd_OnExecute;
        }

        private void SelectItemCmd_OnExecute(UIElement sender)
        {

        }
    }
}