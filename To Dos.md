
###### To Do's
*   Todo: Refactor StartMenuItem into two separate subclasses. One for directories, one for files.
*   Todo: Only directory items should change color when dragged over.
*   Todo: In Saved view, *no* items (including directories) should ever change color when dragged over.
*   Todo: Active view does not refresh its contents from the file system after save.
*   Todo: `SavedStartMenuDataService` should delete old contents when copying new state.
*   Todo: The top level directory objects in Global (e.g. `ActiveStartMenuItems`, `SavedSystemStartMenuItems`, etc.) could be organized into Dictionaries by the type of view they represent (Active, Saved, etc.).