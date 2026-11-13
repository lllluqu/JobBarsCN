using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace JobBars {
    public partial class JobBars {
        public bool Visible = false;

        public static readonly Vector4 RED_COLOR = new( 0.85098039216f, 0.32549019608f, 0.30980392157f, 1.0f );
        public static readonly Vector4 GREEN_COLOR = new( 0.36078431373f, 0.72156862745f, 0.36078431373f, 1.0f );

        private void BuildSettingsUi() {
            if( !Visible ) return;

            var _ID = "##JobBars_Settings";
            ImGui.SetNextWindowSize( new Vector2( 600, 1000 ), ImGuiCond.FirstUseEver );
            if( ImGui.Begin( Loc.T( "JobBars Settings" ), ref Visible ) ) {
                ImGui.BeginTabBar( "Tabs" + _ID );
                if( ImGui.BeginTabItem( Loc.T( "Gauges" ) + _ID ) ) {
                    GaugeManager?.Draw();
                    ImGui.EndTabItem();
                }

                if( ImGui.BeginTabItem( Loc.T( "Icons" ) + _ID ) ) {
                    IconManager?.Draw();
                    ImGui.EndTabItem();
                }

                if( ImGui.BeginTabItem( Loc.T( "Buffs" ) + _ID ) ) {
                    BuffManager?.Draw();
                    ImGui.EndTabItem();
                }

                if( ImGui.BeginTabItem( Loc.T( "Cooldowns" ) + _ID ) ) {
                    CooldownManager?.Draw();
                    ImGui.EndTabItem();
                }

                if( ImGui.BeginTabItem( Loc.T( "Cursor" ) + _ID ) ) {
                    CursorManager?.Draw();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();

                ImGui.Separator();
                ImGui.SetNextItemWidth( 200f );
                if( ImGui.BeginCombo( Loc.T( "Language" ) + _ID, Loc.Display( Configuration.Language ) ) ) {
                    foreach( var language in new[] { PluginLanguage.Auto, PluginLanguage.Chinese, PluginLanguage.English } ) {
                        if( ImGui.Selectable( Loc.Display( language ) + _ID, language == Configuration.Language ) ) {
                            Configuration.Language = language;
                            Loc.SetSetting( language );
                            Configuration.Save();
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            ImGui.End();

            GaugeManager?.DrawPositionBox();
            BuffManager?.DrawPositionBox();
        }

        public static void SetWindowPosition( string Id, Vector2 position ) {
            var minPosition = ImGuiHelpers.MainViewport.Pos;
            ImGui.SetWindowPos( Id, position + minPosition );
        }

        public static bool DrawPositionView( string Id, Vector2 position, out Vector2 newPosition ) {
            ImGuiHelpers.ForceNextWindowMainViewport();
            var minPosition = ImGuiHelpers.MainViewport.Pos;
            ImGui.SetNextWindowPos( position + minPosition, ImGuiCond.FirstUseEver );
            ImGui.SetNextWindowSize( new Vector2( 200, 200 ) );
            ImGui.PushStyleVar( ImGuiStyleVar.Alpha, 0.7f );
            ImGui.Begin( Id, ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize );
            newPosition = ImGui.GetWindowPos() - minPosition;
            ImGui.PopStyleVar( 1 );
            ImGui.End();
            return newPosition != position;
        }

        public static void Separator() {
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 2 );
            ImGui.Separator();
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 2 );
        }

        public static bool RemoveButton( string label, bool small = false ) => ColorButton( label, RED_COLOR, small );

        public static bool OkButton( string label, bool small = false ) => ColorButton( label, GREEN_COLOR, small );

        public static bool ColorButton( string label, Vector4 color, bool small ) {
            var ret = false;
            ImGui.PushStyleColor( ImGuiCol.Button, color );
            if( small ) {
                if( ImGui.SmallButton( label ) ) {
                    ret = true;
                }
            }
            else {
                if( ImGui.Button( label ) ) {
                    ret = true;
                }
            }
            ImGui.PopStyleColor();
            return ret;
        }

        public static bool DrawCombo<T>( T[] validOptions, T currentValue, string label, string _ID, out T newValue ) {
            newValue = currentValue;
            var ret = false;
            if( ImGui.BeginCombo( label + _ID, Loc.Display( currentValue ) ) ) {
                foreach( var value in validOptions ) {
                    if( ImGui.Selectable( Loc.Display( value ) + _ID, value.Equals( currentValue ) ) ) {
                        ret = true;
                        newValue = value;
                    }
                }
                ImGui.EndCombo();
            }
            return ret;
        }
    }
}
