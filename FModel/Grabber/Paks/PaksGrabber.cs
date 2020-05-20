﻿using FModel.Logger;
using FModel.Utils;
using FModel.ViewModels.MenuItem;
using FModel.Windows.CustomNotifier;
using FModel.Windows.Launcher;
using PakReader.Pak;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FModel.Grabber.Paks
{
    static class PaksGrabber
    {
        public static async Task PopulateMenu()
        {
            PopulateBase();

            if (string.IsNullOrEmpty(Properties.Settings.Default.PakPath))
            {
                var launcher = new FLauncher();
                if ((bool)launcher.ShowDialog())
                {
                    Properties.Settings.Default.PakPath = launcher.Path;
                    Properties.Settings.Default.Save();
                }
            }

            // define the current game thank to the pak path
            Folders.SetGameName(Properties.Settings.Default.PakPath);

            // Add Pak Files
            if (Directory.Exists(Properties.Settings.Default.PakPath))
            {
                foreach (string pak in Directory.GetFiles(Properties.Settings.Default.PakPath, "*.pak"))
                {
                    if (pak.Contains("pakChunkEarly-WindowsClient.pak"))
                        continue;
                    
                    if (!Utils.Paks.IsFileReadLocked(new FileInfo(pak)))
                    {
                        PakFileReader pakFile = new PakFileReader(pak);
                        DebugHelper.WriteLine("{0} {1} {2} {3}", "[FModel]", "[PAK]", "[Registering]", $"{pakFile.FileName} with GUID {pakFile.Info.EncryptionKeyGuid.Hex}");

                        MenuItems.pakFiles.Add(new PakMenuItemViewModel
                        {
                            PakFile = pakFile,
                            IsEnabled = false
                        });
                    }
                    else
                    {
                        Globals.gNotifier.ShowCustomMessage(Properties.Resources.PakFiles, string.Format(Properties.Resources.PakFileLocked, Path.GetFileNameWithoutExtension(pak)));
                        DebugHelper.WriteLine("{0} {1} {2} {3}", "[FModel]", "[PAK]", "[Locked]", pak);
                    }
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private static void PopulateBase()
        {
            // Loading Mode
            PakMenuItemViewModel parent = new PakMenuItemViewModel
            {
                Header = $"{Properties.Resources.LoadingMode}   🠞   {Properties.Resources.Default}",
                Icon = new Image { Source = new BitmapImage(new Uri("Resources/progress-download.png", UriKind.Relative)) }
            };
            parent.Childrens = new ObservableCollection<PakMenuItemViewModel>
            {
                new PakMenuItemViewModel // Default Mode
                {
                    Header = Properties.Resources.Default,
                    Parent = parent,
                    IsCheckable = true,
                    IsChecked = true,
                    StaysOpenOnClick = true
                },
                new PakMenuItemViewModel
                {
                    Header = Properties.Resources.NewFiles,
                    Parent = parent,
                    IsCheckable = true,
                    StaysOpenOnClick = true
                },
                new PakMenuItemViewModel
                {
                    Header = Properties.Resources.ModifiedFiles,
                    Parent = parent,
                    IsCheckable = true,
                    StaysOpenOnClick = true
                },
                new PakMenuItemViewModel
                {
                    Header = Properties.Resources.NewModifiedFiles,
                    Parent = parent,
                    IsCheckable = true,
                    StaysOpenOnClick = true
                }
            };
            MenuItems.pakFiles.Add(parent);

            // Load All
            MenuItems.pakFiles.Add(new PakMenuItemViewModel
            {
                Header = Properties.Resources.LoadAll,
                Icon = new Image { Source = new BitmapImage(new Uri("Resources/folder-download.png", UriKind.Relative)) }
            });

            // Separator
            MenuItems.pakFiles.Add(new Separator { });
        }
    }
}