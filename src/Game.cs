using Core;
using System;
using ImGuiNET;


public class Game : IDisposable
{
    private const int WIDTH = 800, HEIGHT = 600;
    private ImGuiController imGuiController = null;
    private IntPtr window;


    public void Run()
    {
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
        imGuiController = new ImGuiController(window);
        imGuiController.Init();

        while (GLFW.glfwWindowShouldClose(window) == 0) {
            GlfwEvents();
            imGuiController.Update();
            ImGuiDemoRender();
            GL.glClear(GL.GL_COLOR_BUFFER_BIT);
            imGuiController.Render();
            GLFW.glfwSwapBuffers(window);
        }
    }

    private void GlfwEvents()
    {
        GLFW.glfwPollEvents();
        if (GLFW.glfwGetKey(window, GLFW.GLFW_KEY_ESCAPE) == GLFW.GLFW_PRESS)
            GLFW.glfwSetWindowShouldClose(window, GLFW.GLFW_TRUE);
    }

    private void ImGuiDemoRender()
    {
        ImGui.Begin("Info");
        ImGui.Text(GL.glGetString(GL.GL_VERSION));
        ImGui.Text($"FPS : {(ImGui.GetIO().Framerate / 20).ToString("0")}");
        if (ImGui.Button("Demo Button"))
            Console.WriteLine("ImGui::ClickButton");
        ImGui.End();
    }

    public void Dispose()
    {
        imGuiController.Dispose();
        GLFW.glfwTerminate();
    }
}