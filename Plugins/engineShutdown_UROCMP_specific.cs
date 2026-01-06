// plugin to add a button for Engine Kill

using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using MissionPlanner;

public class RelayConfirmPlugin : MissionPlanner.Plugin.Plugin
{
    public override string Name   => "Engine KILL";
    public override string Author => "Mckenzie";
    public override string Version => "1.0";

    public override bool Init()   { return true; }

    public override bool Loaded()
    {
        // Add our button to the Actions tab UI after the form is ready
        MainV2.instance.BeginInvoke((MethodInvoker)(() =>
        {
            // Find the Actions tab's main TableLayoutPanel
            var actionsLayout = Host.MainForm.FlightData
                .Controls.Find("tableLayoutPanel1", true)
                .FirstOrDefault() as TableLayoutPanel;

            if (actionsLayout == null) return;

            // Create the button
            var btn = new Button();
            btn.Text = "Engine KILL";
            btn.Width = 160;
            btn.Height = 60;
            btn.BackColor = Color.Red;

            // var btn2 = new Button();
            // btn2.Text = "Engine Start";
            // btn2.Width = 160;
            // btn2.Height = 60;
            // btn2.BackColor = Color.Green;

            btn.Click += async (s, e) =>
            {
                var result = MessageBox.Show(
                    "Are you SURE you want to cut the engine? THE ENGINE CANNOT BE RESTARTED!!!",
                    "WARNING WARNING WARNING",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    // Send MAV_CMD.DO_SET_RELAY: param1 = relay number, param2 = 0/1 (off/on)
                    // Relay0 -> param1 = 0, LOW -> param2 = 0
                    _ = MainV2.comPort.doCommand(
                        (byte)MainV2.comPort.sysidcurrent,
                        (byte)MainV2.comPort.compidcurrent,
                        MAVLink.MAV_CMD.DO_SET_RELAY,
                        0, 0, 0, 0, 0, 0, 0
                    );

                    //wait a few seconds
                    btn.Enabled = false;
                    int seconds = 10;
                    btn.Text = $"Disabled ({seconds}s)";

                    var t = new Timer();
                    t.Interval = 1000;
                    t.Tick += (sender2, e2) =>
                    {
                        seconds--;
                        if (seconds > 0)
                        {
                            btn.Text = $"Disabled ({seconds}s)";
                        }
                        else
                        {
                            t.Stop();
                            t.Dispose();

                            _ = MainV2.comPort.doCommand(
                                (byte)MainV2.comPort.sysidcurrent,
                                (byte)MainV2.comPort.compidcurrent,
                                MAVLink.MAV_CMD.DO_SET_RELAY,
                                0, 1, 0, 0, 0, 0, 0
                            );

                            btn.Text = "Engine KILL";
                            btn.Enabled = true;
                        }
                    };
                    t.Start();
                }
            };

            // btn2.Click += (s, e) =>
            // {
            //     var result = MessageBox.Show(
            //         "Are you SURE you want to start the engine? Make sure the prop is CLEAR!",
            //         "Caution",
            //         MessageBoxButtons.YesNo,
            //         MessageBoxIcon.Warning
            //     );

            //     if (result == DialogResult.Yes)
            //     {
            //         // Send MAV_CMD.DO_SET_RELAY: param1 = relay number, param2 = 0/1 (off/on)
            //         // Relay0 -> param1 = 0, LOW -> param2 = 0
            //         _ = MainV2.comPort.doCommand(
            //             (byte)MainV2.comPort.sysidcurrent,
            //             (byte)MainV2.comPort.compidcurrent,
            //             MAVLink.MAV_CMD.DO_SET_SERVO,
            //             0, 3, 1320, 0, 0, 0, 0
            //         );

            //         //wait for starter
            //         btn2.Enabled = false;
            //         int seconds2 = 10;
            //         btn2.Text = $"Starting ({seconds2}s)";

            //         var t2 = new Timer();
            //         t2.Interval = 1000;
            //         t2.Tick += (sender2, e2) =>
            //         {
            //             seconds2--;
            //             if (seconds2 > 0)
            //             {
            //                 btn2.Text = $"Starting ({seconds2}s)";
            //             }
            //             else
            //             {
            //                 t2.Stop();
            //                 t2.Dispose();

            //                 _ = MainV2.comPort.doCommand(
            //                     (byte)MainV2.comPort.sysidcurrent,
            //                     (byte)MainV2.comPort.compidcurrent,
            //                     MAVLink.MAV_CMD.DO_SET_SERVO,
            //                     0, 3, 1164, 0, 0, 0, 0
            //                 );

            //                 btn2.Text = "Engine Start";
            //                 btn2.Enabled = true;

            //             }
            //         };
            //         t2.Start();
            //     }
            // };

            // Place button into a free cell—community notes mention row 4 has space.
            // If this layout changes in future MP versions, you may need to adjust this.
            int targetColumn = 2;
            int targetRow = 8;
            actionsLayout.Controls.Add(btn, targetColumn, targetRow);
            // int targetColumn2 = 1;
            // int targetRow2 = Math.Min(4, actionsLayout.RowCount - 1);
            // actionsLayout.Controls.Add(btn2, targetColumn2, targetRow2);
        }));

        return true;
    }

    public override bool Exit() { return true; }
    public override bool Loop() { return true; }
}