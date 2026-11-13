using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace JobBars.Localization;

/// <summary>
/// Which language the plugin UI should be rendered in.
/// </summary>
public enum PluginLanguage {
    [Description( "Auto (Follow Client)" )]
    Auto = 0,

    [Description( "Simplified Chinese" )]
    Chinese = 1,

    [Description( "English" )]
    English = 2,
}

/// <summary>
/// Tiny runtime localization layer. English source text is the lookup key so that every call
/// site stays readable and untranslated strings gracefully fall back to English.
/// </summary>
public static class Loc {
    private const string ResourceName = "JobBars.Localization.zh-CN.json";

    private static Dictionary<string, string> _zh = new( StringComparer.Ordinal );
    private static bool _loaded;
    private static readonly HashSet<string> MissingKeys = new( StringComparer.Ordinal );
    private static readonly Dictionary<string, string> EnumCache = new( StringComparer.Ordinal );

    /// <summary>Configured language mode.</summary>
    public static PluginLanguage Setting { get; private set; } = PluginLanguage.Auto;

    /// <summary>True when the plugin should currently render Simplified Chinese.</summary>
    public static bool IsChinese { get; private set; }

    /// <summary>Raised when the effective language changed, so caches can be dropped.</summary>
    public static event Action? LanguageChanged;

    public static void Initialize( PluginLanguage setting ) {
        EnsureInitialized();
        var previous = IsChinese;
        Setting = setting;
        IsChinese = Resolve( setting );
        ApplyChange( previous );
    }

    public static void SetSetting( PluginLanguage setting ) {
        EnsureInitialized();
        var previous = IsChinese;
        Setting = setting;
        IsChinese = Resolve( setting );
        ApplyChange( previous );
    }

    private static void ApplyChange( bool previous ) {
        EnumCache.Clear();

        if( previous == IsChinese )
            return;

        try {
            global::JobBars.Helper.UiHelper.InvalidateLocalizationCache();
        }
        catch {
            // ignored, the helper may not be initialized yet
        }

        LanguageChanged?.Invoke();
    }

    private static bool Resolve( PluginLanguage setting ) => setting switch {
        PluginLanguage.Chinese => true,
        PluginLanguage.English => false,
        _ => DetectChineseClient()
    };

    /// <summary>Translates a single string. Unknown strings are returned unchanged.</summary>
    public static string T( string english ) {
        EnsureInitialized();

        if( !IsChinese || string.IsNullOrEmpty( english ) )
            return english;

        if( _zh.TryGetValue( english, out var translated ) )
            return translated;

        if( MissingKeys.Add( english ) )
            LogDebug( $"[JobBars] Missing zh-CN translation: {english}" );

        return english;
    }

    private static void LogDebug( string message ) {
        try {
            Dalamud.PluginLog?.Debug( message );
        }
        catch {
            // logging must never break the UI
        }
    }

    /// <summary>Translates a composite format string and applies <paramref name="args"/>.</summary>
    public static string T( string english, params object?[] args ) {
        var format = T( english );
        if( args.Length == 0 )
            return format;

        try {
            return string.Format( CultureInfo.InvariantCulture, format, args );
        }
        catch( FormatException ) {
            return format;
        }
    }

    /// <summary>Looks up a translation without recording a missing-key diagnostic.</summary>
    public static bool TryGet( string english, out string translated ) {
        EnsureInitialized();

        if( IsChinese && _zh.TryGetValue( english, out var value ) ) {
            translated = value;
            return true;
        }

        translated = english;
        return false;
    }

    /// <summary>
    /// Resolves the display text for a value. Enums are translated through their
    /// <see cref="DescriptionAttribute"/> (or member name) so combo boxes read naturally.
    /// </summary>
    public static string Display( object? value ) {
        if( value is null )
            return string.Empty;

        var type = value.GetType();
        if( !type.IsEnum )
            return value.ToString() ?? string.Empty;

        if( value is global::JobBars.Data.JobIds job )
            return global::JobBars.Helper.UiHelper.JobDisplay( job );

        var key = type.FullName + "|" + value;
        if( EnumCache.TryGetValue( key, out var cached ) )
            return cached;

        var english = value.ToString() ?? string.Empty;
        try {
            var field = type.GetField( english );
            var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if( !string.IsNullOrEmpty( description ) )
                english = description;
        }
        catch {
            // ignored, fall back to the member name
        }

        var display = T( english );
        EnumCache[key] = display;
        return display;
    }

    private static void EnsureInitialized() {
        if( _loaded )
            return;

        LoadDictionary();
        IsChinese = Resolve( Setting );
    }

    private static void LoadDictionary() {
        if( _loaded )
            return;

        _loaded = true;

        try {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream( ResourceName );
            if( stream is null ) {
                LogDebug( $"[JobBars] Localization resource '{ResourceName}' not found." );
                return;
            }

            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>( stream );
            if( parsed is null || parsed.Count == 0 ) {
                LogDebug( "[JobBars] Localization resource is empty." );
                return;
            }

            _zh = new Dictionary<string, string>( parsed, StringComparer.Ordinal );
            LogDebug( $"[JobBars] Loaded {_zh.Count} zh-CN strings." );
        }
        catch( Exception ex ) {
            LogDebug( $"[JobBars] Failed to load zh-CN localization: {ex.Message}" );
        }
    }

    private static bool DetectChineseClient() {
        try {
            var uiLanguage = Dalamud.PluginInterface.UiLanguage;
            if( !string.IsNullOrEmpty( uiLanguage ) &&
                uiLanguage.StartsWith( "zh", StringComparison.OrdinalIgnoreCase ) )
                return true;
        }
        catch {
            // ignored, fall through to the next signal
        }

        try {
            if( Dalamud.ClientState.ClientLanguage.ToString()
                .Contains( "Chinese", StringComparison.OrdinalIgnoreCase ) )
                return true;
        }
        catch {
            // ignored
        }

        try {
            var sample = Dalamud.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Item>().GetRow( 1 ).Name.ToString();
            return ContainsCjk( sample );
        }
        catch {
            return false;
        }
    }

    private static bool ContainsCjk( string? text ) {
        if( string.IsNullOrEmpty( text ) )
            return false;

        foreach( var c in text ) {
            if( c >= '\u4E00' && c <= '\u9FFF' )
                return true;
        }

        return false;
    }
}
