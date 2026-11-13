using Dalamud.Bindings.ImGui;
using JobBars.Data;
using System;

namespace JobBars.Cursors.Manager {
    public partial class CursorManager {
        private static readonly CursorPositionType[] ValidCursorPositionType = ( CursorPositionType[] )Enum.GetValues( typeof( CursorPositionType ) );

        private readonly InfoBox<CursorManager> HideWhenInfoBox = new() {
            Label = "Hide When",
            ContentsAction = ( CursorManager manager ) => {
                if( ImGui.Checkbox( Loc.T( "Mouse held" ), ref JobBars.Configuration.CursorHideWhenHeld ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( Loc.T( "Out of combat" ), ref JobBars.Configuration.CursorHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( Loc.T( "Weapon sheathed" ), ref JobBars.Configuration.CursorHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( Loc.T( "Cursor enabled" ) + Id, ref JobBars.Configuration.CursorsEnabled ) ) JobBars.Configuration.Save();
        }

        protected override void DrawSettings() {
            HideWhenInfoBox.Draw( this );

            if( JobBars.DrawCombo( ValidCursorPositionType, JobBars.Configuration.CursorPosition, Loc.T( "Cursor positioning" ), Id, out var newPosition ) ) {
                JobBars.Configuration.CursorPosition = newPosition;
                JobBars.Configuration.Save();
            }

            if( JobBars.Configuration.CursorPosition == CursorPositionType.CustomPosition ) {
                if( ImGui.InputFloat2( Loc.T( "Custom cursor position" ), ref JobBars.Configuration.CursorCustomPosition ) ) {
                    JobBars.Configuration.Save();
                }
            }

            if( ImGui.InputFloat( Loc.T( "Inner scale" ) + Id, ref JobBars.Configuration.CursorInnerScale ) ) JobBars.Configuration.Save();
            if( ImGui.InputFloat( Loc.T( "Outer scale" ) + Id, ref JobBars.Configuration.CursorOuterScale ) ) JobBars.Configuration.Save();

            if( Configuration.DrawColor( Loc.T( "Inner color" ), InnerColor, out var newColorInner ) ) {
                InnerColor = newColorInner;
                JobBars.Configuration.CursorInnerColor = newColorInner.Name;
                JobBars.Configuration.Save();
                Root?.SetInnerColor( InnerColor );
            }

            if( Configuration.DrawColor( Loc.T( "Outer color" ), OuterColor, out var newColorOuter ) ) {
                OuterColor = newColorOuter;
                JobBars.Configuration.CursorOuterColor = newColorOuter.Name;
                JobBars.Configuration.Save();
                Root?.SetOuterColor( OuterColor );
            }
        }

        protected override void DrawItem( Cursor item, JobIds _ ) {
            item.Draw( Id );
        }
    }
}
