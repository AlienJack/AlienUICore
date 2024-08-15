using AlienUI.Core.Commnands;
using AlienUI.Models;

namespace AlienUI.UIElements
{
    public class HUD : UserControl
    {
        public override ControlTemplate DefaultTemplate => new ControlTemplate("Builtin.HUD");


        public bool CanBeBack
        {
            get { return (bool)GetValue(CanBeBackProperty); }
            set { SetValue(CanBeBackProperty, value); }
        }

        public static readonly DependencyProperty CanBeBackProperty =
            DependencyProperty.Register("CanBeBack", typeof(bool), typeof(HUD), new PropertyMetadata(false).AmlDisable());



        public Command BackCmd
        {
            get { return (Command)GetValue(BackCmdProperty); }
            set { SetValue(BackCmdProperty, value); }
        }

        public static readonly DependencyProperty BackCmdProperty =
            DependencyProperty.Register("BackCmd", typeof(Command), typeof(HUD), new PropertyMetadata(null).AmlDisable());

        protected override void OnInitialized()
        {
            CanBeBack = UIManager.Instance.CanHUDBack(this); 
            BackCmd = new Command();
            BackCmd.OnExecute += BackCmd_OnExecute;
        }

        private void BackCmd_OnExecute()
        {
            UIManager.Instance.HUDBack(this);
        }

    }
}
