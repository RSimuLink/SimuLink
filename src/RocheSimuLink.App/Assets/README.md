# App brand assets

`roche-logo.png` — the Roche logo shown in the top-right of the main toolbar.

The app loads this file at runtime from `Assets/roche-logo.png` next to the
executable. If it is missing, the brand area is left blank (startup is not
affected).

Recommended: a transparent-background PNG, roughly 2:1 aspect ratio (the
toolbar displays it at ~80×40 px with `Zoom` scaling, so a larger source such
as 800×400 looks crisp on high-DPI displays).
