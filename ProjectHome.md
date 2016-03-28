The project is a WPF MVVM compliant controls library.

The library currently contains:

1.Implementation of AutoComplete/AutoSuggest TextBox
The control is designed to hook up to a TextBox and provide users with suggestons as they type in the TextBox. There is also a test application which shows you how to use the control in your projects.

AutoSuggest/AutoComplete Control features:
  * MVVM compliant. The ViewModel is completely functional without user interface;
  * Full control over the visual presentation of the control by utilizing standard WPF styles and templates.
  * Hooks up to any TextBox and provides AutoSuggest, AutoComplete or both functionality;
  * Option allowing submission of "free text" and upon confirmation the "free text" can be converted to a valid value;
  * Supports commands injection in the suggestion’s window (this is useful when you want to allow the users to edit or create new suggestions from within the suggestion’s control);
  * The control presenting the suggestions can be WPF DataGrid or ListView;
  * The control's functionality is controlled through properties of the ViewModel. (Note that the control would work only if its DataContext is of type AutoSuggestViewModel). Amongst other things this will allow you to serialize the control's settings and thus allow you to provide ways to your users to personalize the control.
  * Suggestions filtering algorithm can be injected by the user. This allows third party users to implement an algorithm which fetches the suggested values from a webservice or from a database, to do cleverer filtering overcoming spelling mistakes and auto-correction, etc. A default implementation of the filtering algorithm which works with a collection of all possible suggestions is provided;
  * Supports walking up and down the suggestions with the arrow keys and that is done without losing TextBox focus!;
  * Supports the following options for "Cancel" and "Select" behavior:
    1. Space may act as selection command or exit command. If it acts as a selection command, it should only select an item if it is the last item in the suggestions list and if the option to allow “free text” values is off.
    1. Tab may act as a “Select” or a “Cancel” command. Lost focus is also considered Tab in that sense.
    1. Enter may act as a “Select” or a “Cancel” command.
    1. The arrow keys may act as a “Select” command.
  * Supports delay in milliseconds before invoking the filtering algorithm. This is useful if the filtering algorithm is heavy and you do not want to invoke it every time the user types a character but want the user to pause and slow down before you invoke the algorithm;
  * Option which allows or disallows empty values. Also by default, an "empty value" is the "null" value but users may supply a different value to represented the "empty value";
  * Provides the AutoComplete feature which highlights the completed text after the point where user stopped typing. The features is controlled by an option.

Future development: <br />
There are more features on which we are working on and which will come out soon. Some of the major things we are working is to make the control work with more advanced search criteria and allowing the users to invoke the filtering algorithm manually or via a click of a button. These features will make the control be positioned as an alternative to the lookup controls widely used, which have a TextBox and a search button where the search button invokes a popup window.
We are also working on a control which will allow for multiple suggestions to be picked and on DataGridColumn to host the AutoSuggestControl which will allow you to use the control in a WPF DataGrid.

2. WPF utility classes bundled in their own library.

If you like the KOControls library please consider making a donation to help us improve the existing controls and write new useful controls.

Checkout the code project article here for more information about the design and how to use the library: http://www.codeproject.com/KB/WPF/AutoSuggest_AutoComplete.aspx





