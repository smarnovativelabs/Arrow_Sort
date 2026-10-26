# Unified menu navigation

Home, Shop and Map use one authored WoolMenuNavigation with three selected-state sprites. Legacy page menus are hidden. Shop viewport clears the bar. Map artwork fills the viewport behind the transparent rounded navigation, with a cream/gold heading and arrows.

Assets: Assets/WoolGame/Designer/UI/Navigation. SharedMenuAtlas.png uses RGBA32 output to preserve transparent corners.

Validated: all three tab callbacks, one shared instance, matching geometry on each page, transparent corner alpha zero, settled tablet map capture, phone aspect geometry and full map viewport. No Android build performed.
