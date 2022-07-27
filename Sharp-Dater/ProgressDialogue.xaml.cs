using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sharp_Dater
{
    /// <summary>
    /// Interaction logic for ProgressDialogue.xaml
    /// </summary>
    public partial class ProgressDialogue : Window
    {
        public void SetProgress(float p)
        {
            if (p > 100)
                p = 100;

            
        }

        public ProgressDialogue()
        {
            InitializeComponent();
        }
    }
}
