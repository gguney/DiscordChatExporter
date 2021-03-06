﻿using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Helpers;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Framework;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs
{
    public class ExportSetupViewModel : DialogScreen
    {
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;

        public GuildViewModel Guild { get; set; }

        public IReadOnlyList<ChannelViewModel> Channels { get; set; }

        public bool IsSingleChannel => Channels.Count == 1;

        public string OutputPath { get; set; }

        public IReadOnlyList<ExportFormat> AvailableFormats =>
            Enum.GetValues(typeof(ExportFormat)).Cast<ExportFormat>().ToArray();

        public ExportFormat SelectedFormat { get; set; } = ExportFormat.HtmlDark;

        public DateTimeOffset? After { get; set; }

        public DateTimeOffset? Before { get; set; }

        public int? PartitionLimit { get; set; }

        public ExportSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
        {
            _dialogManager = dialogManager;
            _settingsService = settingsService;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Persist preferences
            SelectedFormat = _settingsService.LastExportFormat;
            PartitionLimit = _settingsService.LastPartitionLimit;
        }

        public void Confirm()
        {
            // Persist preferences
            _settingsService.LastExportFormat = SelectedFormat;
            _settingsService.LastPartitionLimit = PartitionLimit;

            // Clamp 'after' and 'before' values
            if (After > Before)
                After = Before;
            if (Before < After)
                Before = After;

            // If single channel - prompt file path
            if (IsSingleChannel)
            {
                // Get single channel
                var channel = Channels.Single();

                // Generate default file name
                var defaultFileName = ExportHelper.GetDefaultExportFileName(SelectedFormat, Guild, channel, After, Before);

                // Generate filter
                var ext = SelectedFormat.GetFileExtension();
                var filter = $"{ext.ToUpperInvariant()} files|*.{ext}";

                // Prompt user
                OutputPath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);
            }
            // If multiple channels - prompt dir path
            else
            {
                // Prompt user
                OutputPath = _dialogManager.PromptDirectoryPath();
            }

            // If canceled - return
            if (OutputPath.IsNullOrWhiteSpace())
                return;

            // Close dialog
            Close(true);
        }
    }
}