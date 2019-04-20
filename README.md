##   Introduction
Start Menu Protector is a utility which monitors the state of the Windows Start Menu and reverts
unwanted changes made to it. It allows the user to select a preferred state for the Start Menu,
and then maintains that state by deleting added items the user has previously elected to exclude,
quarantining any new items for which the user hasn't expressed a preference, and ensuring items
which the user _has_ acknowledged remain in their preferred location.

##   Overview
Start Menu Protector's UI can be divided into four top-level areas ("tabs"): **Active**,
**Saved**, **Quarantine**, and **Excluded**. Each of these is made up of two further subviews: **System**
and **User**. Each of the top level tab views represents a set of Start Menu items: 
*   **Active** represents the current items that the user would see in their Start Menu,
the state of which is being preserved from any outside changes to ensure it matches the current
**Saved** set. A background process monitors the state of the Start Menu and reverts modifications
made by the system or other apps. The user can, however, make changes to the Start Menu from
_within_ this view and save them as the new **Saved** state (see **Using Start Menu Protector** below).
*   **Saved** represents the state of the Start Menu that the user has elected to preserve.
Start Menu Protector continuously ensures the state of the Start Menu on the user's system matches
the state of **Saved**.
*   **Quarantine** This view shows all Start Menu folders and shortcuts found in the Start Menu for
which Start Menu Protector does not have a corresponding desired state set by the user. A user
can elect to **Exclude** them, or else specify a location for them and integrate them into the **Saved** set.
*   **Excluded** This view represents all items the user has elected to permanently exclude from the Start
Menu. If Start Menu protector detects that a directory matching the name and contents of an excluded directory
or a shortcut file matching the name and target of an excluded shortcut file has been added to the active
Start Menu it will delete it.

##  Using Start Menu Protector
####    Active View
In the **Active** view, a user can:
*   Make changes to the desired Start Menu state by manipulating the UI. Available actions include:
	*   **Dragging and dropping** items (files or directories) onto directories to move them.
	*   **Excluding** items by highlighting (clicking) them, and then pressing the **Delete** or **Backspace**
	 key. Excluded items are not copied to the **Save** items on the next **Save** action, but are instead
	 copied to the **Excluded** items.
*   Save the current state of the Start Menu (including any changes made from within Start Menu Protector) to
be the new desired **Saved** state. This is done by pressing the **Save** button.
####    Saved View
The **Saved** view is currently read only.
####    Quarantine View
In the **Quarantine** view, a user can:
*   Choose what to do with any quarantined items by manipulating the UI. Available actions include:
	*   **Excluding** items by highlighting (clicking) them, and then pressing the **Delete** or **Backspace**
	 key. Unlike in the **Active** view, excluded items are immediately moved to the **Excluded** view as soon
	 as the **Delete** or **Backspace** key is pressed (there is no **Save** button in the **Quarantine** view).
####    Excluded View
Much like the **Saved** view, the **Excluded** view is largely intended to be read only. However, a user who
has mistakenly excluded an item can use this view to return it to the **Quarantine** view. They can do this by:
*   Highlighting (clicking) them, and then pressing the **Insert** key. The item will then reappear in the
**Quarantine** view.

##  Other Behaviors
*   If Start Menu Protector detects a shortcut or folder in an unrecognized location in the active Start Menu
that has a name that matches an existing item for which the user has registered a preferred location (a "saved" item):
	*   Start Menu Protector first checks if the item has contents that match the registered item (if it is a
	directory) or if its target matches the target of the registered shortcut (if it is a shortcut file).
		*   If this condition is not met, the item is quarantined.
		*   If this condition *is* met, the item is moved to the location that was registered for the matching
		saved item.


