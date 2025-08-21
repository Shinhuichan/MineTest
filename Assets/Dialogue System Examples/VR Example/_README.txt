Basic Unity XR Interaction - Dialogue System Example
Updated: 2025-04-17

Exported from:
- Unity 6.0
- XR Interaction Toolkit 3.0.8
- Dialogue System for Unity 2.2.51

You MUST install the Dialogue System, Input System, XR Interaction Toolkit, and at least
one XR provider such as Oculus XR.

You MUST install set up a Provider in Edit > Project Settings > XR Plug-in Management,
such as OpenXR. If you're using an Oculus device, in the Oculus Meta app select 
Settings > General and Set Meta Quest Link to act as the OpenXR Runtime.

Since the XR Interaction Toolkit is built on the new Input System package, you must enable the
Dialogue System's Input System integration. In the Welcome Window, tick USE_NEW_INPUT.

If you are using URP, you must download and import the Demo scene's URP materials from the
Demo's subfolder.

FEATURES:

- Left stick moves, right stick rotates 90 degrees.

- Dialogue Manager GameObject has two canvases: 
	- Screen space overlay: For alert panel.
	- World space: For dialogue panel. Expected to be moved by NPC when conversation starts.
	
- Terminal conversation: Demonstrates a world space dialogue UI.

- Private Hart conversation: Demonstrates a world space subtitle panel, and uses a simple
  script named MoveDialoguePanel to move the Dialogue Manager's dialogue panel next to the NPC
  for the response menu.
