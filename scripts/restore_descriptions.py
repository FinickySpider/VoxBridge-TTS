#!/usr/bin/env python3
"""
Restore image descriptions
"""

from pathlib import Path

# Mapping of image filenames to their descriptions
IMAGE_DESCRIPTIONS = {
    "first-run-settings.png": "First-run settings window showing device selection dropdowns and test buttons.",
    "tray-icon.png": "System tray area showing the TTS Communication Tool icon (a speech bubble icon) highlighted.",
    "overlay-typing.png": "The overlay window with typed text and the Send button visible.",
    "hero-overview.png": "Main application showing the system tray icon, overlay input window, and settings window demonstrating the core workflow.",
    "phrase-manager.png": "Phrase Manager showing the list of saved phrases with search, category filter, and favorites toggle.",
    "phrase-editor.png": "Phrase Editor window showing text fields, voice override options, and preview controls.",
    "settings-window.png": "Settings window showing the tabbed interface with General, Hotkeys, Audio, Voice, Phrases, Appearance, and Theme tabs.",
    "settings-replacements.png": "Replacements tab showing a list of text replacement rules with trigger/replacement fields and enable toggles.",
    "settings-theme.png": "Theme editor showing the colour swatches, preview pane, and preset dropdown.",
    "settings-voice-elevenlabs.png": "Voice settings tab showing ElevenLabs engine selected with API key entry field and voice dropdown.",
}

DOCS_DIR = Path("docs/retype")

def restore_descriptions():
    """Restore image descriptions in all markdown files"""
    for md_file in DOCS_DIR.rglob("*.md"):
        # Skip .retype directory (generated output)
        if ".retype" in str(md_file):
            continue
            
        content = md_file.read_text(encoding="utf-8")
        new_content = content
        
        for filename, description in IMAGE_DESCRIPTIONS.items():
            # Find patterns like ![...](.../filename.png) or ![...](filename.png)
            pattern = f'!\\[\\.\\.\\.\\]\\(.*{filename}\\)'
            import re
            if re.search(pattern, new_content):
                # Determine the path prefix
                if "../screenshots/" in new_content:
                    replacement = f'![{description}](../screenshots/{filename})'
                elif "screenshots/" in new_content:
                    replacement = f'![{description}](screenshots/{filename})'
                else:
                    # Try to find the actual path
                    import re
                    match = re.search(f'!\\[\\.\\.\\.\\]\\((.*{filename})\\)', new_content)
                    if match:
                        path = match.group(1)
                        replacement = f'![{description}]({path})'
                    else:
                        continue
                
                new_content = re.sub(f'!\\[\\.\\.\\.\\]\\(.*{filename}\\)', replacement, new_content)
        
        if content != new_content:
            md_file.write_text(new_content, encoding="utf-8")
            print(f"Updated: {md_file.relative_to(DOCS_DIR)}")

if __name__ == "__main__":
    restore_descriptions()