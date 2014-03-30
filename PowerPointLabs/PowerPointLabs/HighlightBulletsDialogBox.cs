﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PowerPointLabs
{
    public partial class HighlightBulletsDialogBox : Form
    {
        public delegate void UpdateSettingsDelegate(Color highlightColor, Color defaultColor);
        public UpdateSettingsDelegate SettingsHandler;
        public HighlightBulletsDialogBox()
        {
            InitializeComponent();
        }

        public HighlightBulletsDialogBox(Color defaultHighlightColor, Color defaultTextColor)
            : this()
        {
            this.pictureBox1.BackColor = defaultHighlightColor;
            this.pictureBox2.BackColor = defaultTextColor;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = this.pictureBox1.BackColor;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                this.pictureBox1.BackColor = colorDialog.Color;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SettingsHandler(this.pictureBox1.BackColor, this.pictureBox2.BackColor);
            this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = this.pictureBox2.BackColor;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                this.pictureBox2.BackColor = colorDialog.Color;
            }
        }
    }
}
