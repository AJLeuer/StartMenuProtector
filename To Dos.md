
###### To Do's
*   Todo: Items deleted in Active view are being copied to Saved.
*   Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files.
*   Todo: Only directory items should change color when dragged over.
*   Todo: In Saved view, *no* items (including directories) should ever change color when dragged over.
*   Todo: Clicking on the "User" or "System" view reloads all items, but clicking on Active, Saved, etc. tabs does not.
*   Todo: Active view does not refresh its contents from the file system after save.
*   Todo: `SavedStartMenuDataService` should delete old contents when copying new state.
*   Todo: `ActiveViewController` should enter a new state if it detects the user has made a change to the active view.
*   Todo: Normally, the controller overwrites the contents of the active view with the start menu loaded from the OS environment any time the Active view is reloaded. In this state however, the should *not* overwrite the contents of the Active view. This state should last from the time the first  is made, and only end when the user saves.
*   Todo: The top level directory objects in Global (e.g. `ActiveStartMenuItems`, `SavedSystemStartMenuItems`, etc.) could be organized into Dictionaries by the type of view they represent (Active, Saved, etc.).