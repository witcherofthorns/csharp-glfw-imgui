# C# GLFW + IMGUI 
<img src="https://github.com/witcherofthorns/csharp-glfw-imgui/blob/master/gif.gif" width=80% />

This is a project for cross-platform window creation, OpenGL context creation and input control, implemented in C#</br>
It is a ported library from the native C language to my favorite C# language</br>

The built-in ImGui in this project has been adapted for GLFW and GL context, you can use this without worrying about problems :)</br>
All ImGui draw calls are called from the classic Nuget ImGui.Net package

## How to use
Create GLFW window and GL context</br>
```Csharp
private IntPtr window;
  // any void() { ... }
  if (GLFW.glfwInit() == 0) throw new Exception("glfwInit");
  GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 4);
  GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 6);
  GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
  window = GLFW.glfwCreateWindow(WIDTH, HEIGHT, "Window GLFW", IntPtr.Zero, IntPtr.Zero);

  if (window == IntPtr.Zero) {
      GLFW.glfwTerminate();
      throw new Exception("glfwCreateWindow");
  }

  GLFW.glfwMakeContextCurrent(window);
  GL.LoadEntryPoints();
```
<br/>

Create ImGui with GLFW context<br/>
```Csharp
private ImGuiController imGuiController = null;
  // any void() { ... }
  imGuiController = new ImGuiController(window);
  imGuiController.Init();
```
<br/>

Main loop of GLFW and ImGui draw-calls
```Csharp
while (GLFW.glfwWindowShouldClose(window) == 0) {
  GlfwEvents();
  imGuiController.Update();
  // your ImGui calls
  // ImGui.Begin("Info");
  // ImGui.End();     
  GL.glClear(GL.GL_COLOR_BUFFER_BIT);
  imGuiController.Render();
  GLFW.glfwSwapBuffers(window);
}
```
