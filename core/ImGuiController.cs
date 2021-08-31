using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Core
{
    // for global
    using static GLFW;
    using static GL;

    public class ImGuiController : IDisposable
    {
        private readonly IntPtr _window;
        private ImGuiIOPtr _io;

        private double _time;
        private readonly bool[] _mouseJustPressed;
        private readonly IntPtr[] _mouseCursors;

        private uint _fontTexture;
        private uint _shaderHandle, _vertHandle, _fragHandle;
        private int _attribLocationTex, _attribLocationProjMtx;
        private uint _attribLocationVtxPos, _attribLocationVtxUv, _attribLocationVtxColor;
        private uint _vboHandle, _elementsHandle;

        private GLFWmousebuttonfun _userCallbackMousebutton;
        private GLFWscrollfun _userCallbackScroll;
        private GLFWkeyfun _userCallbackKey;
        private GLFWcharfun _userCallbackChar;

        private delegate void SetClipboardTextFn(IntPtr userData, string text);
        private delegate void GetClipboardTextFn(IntPtr userData);

        private readonly SetClipboardTextFn _setClipboardTextFn;
        private readonly GetClipboardTextFn _getClipboardTextFn;

        public ImGuiController(IntPtr window)
        {
            _window = window;

            ImGui.CreateContext();
            ImGui.StyleColorsDark();

            _io = ImGui.GetIO();
            _mouseJustPressed = new bool[(int) ImGuiMouseButton.COUNT];
            _mouseCursors = new IntPtr[(int) ImGuiMouseCursor.COUNT];

            _setClipboardTextFn = (data, text) => glfwSetClipboardString(data, text);
            _getClipboardTextFn = data => glfwGetClipboardString(data);
        }

        public void Init()
        {
            _io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            _io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            _io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            InitGlfw();
            CreateDeviceObjects();
        }

        private void InitGlfw()
        {
            _io.KeyMap[(int) ImGuiKey.Tab] = GLFW_KEY_TAB;
            _io.KeyMap[(int) ImGuiKey.LeftArrow] = GLFW_KEY_LEFT;
            _io.KeyMap[(int) ImGuiKey.RightArrow] = GLFW_KEY_RIGHT;
            _io.KeyMap[(int) ImGuiKey.UpArrow] = GLFW_KEY_UP;
            _io.KeyMap[(int) ImGuiKey.DownArrow] = GLFW_KEY_DOWN;
            _io.KeyMap[(int) ImGuiKey.PageUp] = GLFW_KEY_PAGE_UP;
            _io.KeyMap[(int) ImGuiKey.PageDown] = GLFW_KEY_PAGE_DOWN;
            _io.KeyMap[(int) ImGuiKey.Home] = GLFW_KEY_HOME;
            _io.KeyMap[(int) ImGuiKey.End] = GLFW_KEY_END;
            _io.KeyMap[(int) ImGuiKey.Insert] = GLFW_KEY_INSERT;
            _io.KeyMap[(int) ImGuiKey.Delete] = GLFW_KEY_DELETE;
            _io.KeyMap[(int) ImGuiKey.Backspace] = GLFW_KEY_BACKSPACE;
            _io.KeyMap[(int) ImGuiKey.Space] = GLFW_KEY_SPACE;
            _io.KeyMap[(int) ImGuiKey.Enter] = GLFW_KEY_ENTER;
            _io.KeyMap[(int) ImGuiKey.Escape] = GLFW_KEY_ESCAPE;
            _io.KeyMap[(int) ImGuiKey.KeyPadEnter] = GLFW_KEY_KP_ENTER;
            _io.KeyMap[(int) ImGuiKey.A] = GLFW_KEY_A;
            _io.KeyMap[(int) ImGuiKey.C] = GLFW_KEY_C;
            _io.KeyMap[(int) ImGuiKey.V] = GLFW_KEY_V;
            _io.KeyMap[(int) ImGuiKey.X] = GLFW_KEY_X;
            _io.KeyMap[(int) ImGuiKey.Y] = GLFW_KEY_Y;
            _io.KeyMap[(int) ImGuiKey.Z] = GLFW_KEY_Z;

            _io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_setClipboardTextFn);
            _io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(_getClipboardTextFn);
            _io.ClipboardUserData = _window;

            _mouseCursors[(int) ImGuiMouseCursor.Arrow] = glfwCreateStandardCursor(GLFW_ARROW_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.TextInput] = glfwCreateStandardCursor(GLFW_IBEAM_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.ResizeNS] = glfwCreateStandardCursor(GLFW_VRESIZE_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.ResizeEW] = glfwCreateStandardCursor(GLFW_HRESIZE_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.Hand] = glfwCreateStandardCursor(GLFW_HAND_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.ResizeAll] = glfwCreateStandardCursor(GLFW_ARROW_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.ResizeNESW] = glfwCreateStandardCursor(GLFW_ARROW_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.ResizeNWSE] = glfwCreateStandardCursor(GLFW_ARROW_CURSOR);
            _mouseCursors[(int) ImGuiMouseCursor.NotAllowed] = glfwCreateStandardCursor(GLFW_ARROW_CURSOR);

            _userCallbackMousebutton = MouseButtonCallback;
            _userCallbackScroll = ScrollCallback;
            _userCallbackKey = KeyCallback;
            _userCallbackChar = CharCallback;

            glfwSetMouseButtonCallback(_window, _userCallbackMousebutton);
            glfwSetScrollCallback(_window, _userCallbackScroll);
            glfwSetKeyCallback(_window, _userCallbackKey);
            glfwSetCharCallback(_window, _userCallbackChar);
        }

        private void MouseButtonCallback(IntPtr window, int button, int action, int mods)
        {
            if (action == GLFW_PRESS && button >= 0 && button < _mouseJustPressed.Length)
                _mouseJustPressed[button] = true;
        }

        private void ScrollCallback(IntPtr window, double xo, double yo)
        {
            _io.MouseWheelH += (float) xo;
            _io.MouseWheel += (float) yo;
        }

        private void KeyCallback(IntPtr window, int key, int scancode, int action, int mods)
        {
            if (action == GLFW_PRESS)
                _io.KeysDown[key] = true;
            if (action == GLFW_RELEASE)
                _io.KeysDown[key] = false;

            _io.KeyCtrl = _io.KeysDown[GLFW_KEY_LEFT_CONTROL] || _io.KeysDown[GLFW_KEY_RIGHT_CONTROL];
            _io.KeyShift = _io.KeysDown[GLFW_KEY_LEFT_SHIFT] || _io.KeysDown[GLFW_KEY_RIGHT_SHIFT];
            _io.KeyAlt = _io.KeysDown[GLFW_KEY_LEFT_ALT] || _io.KeysDown[GLFW_KEY_RIGHT_ALT];
            _io.KeySuper = _io.KeysDown[GLFW_KEY_LEFT_SUPER] || _io.KeysDown[GLFW_KEY_RIGHT_SUPER];
        }

        private void CharCallback(IntPtr window, uint c)
        {
            _io.AddInputCharacter(c);
        }

        private void CreateDeviceObjects()
        {
            glGetIntegerv(GL_TEXTURE_BINDING_2D, out var lastTexture);
            glGetIntegerv(GL_ARRAY_BUFFER_BINDING, out var lastArrayBuffer);
            glGetIntegerv(GL_VERTEX_ARRAY_BINDING, out var lastVertexArray);

            const string vs = @"#version 330 core
            layout (location = 0) in vec2 Position;
            layout (location = 1) in vec2 UV;
            layout (location = 2) in vec4 Color;
            uniform mat4 ProjMtx;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy,0,1);
            }";

            const string fs = @"#version 330 core
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            layout (location = 0) out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";

            _vertHandle = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(_vertHandle, 1, new[] {vs}, vs.Length);
            glCompileShader(_vertHandle);
            CheckShader(_vertHandle);

            _fragHandle = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(_fragHandle, 1, new[] {fs}, fs.Length);
            glCompileShader(_fragHandle);
            CheckShader(_fragHandle);

            _shaderHandle = glCreateProgram();
            glAttachShader(_shaderHandle, _vertHandle);
            glAttachShader(_shaderHandle, _fragHandle);
            glLinkProgram(_shaderHandle);
            CheckProgram(_shaderHandle);

            _attribLocationTex = glGetUniformLocation(_shaderHandle, "Texture");
            _attribLocationProjMtx = glGetUniformLocation(_shaderHandle, "ProjMtx");
            _attribLocationVtxPos = (uint) glGetAttribLocation(_shaderHandle, "Position");
            _attribLocationVtxUv = (uint) glGetAttribLocation(_shaderHandle, "UV");
            _attribLocationVtxColor = (uint) glGetAttribLocation(_shaderHandle, "Color");

            glGenBuffers(1, out _vboHandle);
            glGenBuffers(1, out _elementsHandle);

            CreateFontsTexture();

            glBindTexture(GL_TEXTURE_2D, (uint) lastTexture);
            glBindBuffer(GL_ARRAY_BUFFER, (uint) lastArrayBuffer);
            glBindVertexArray((uint) lastVertexArray);
        }

        private void CheckShader(uint handle)
        {
            glGetShaderiv(handle, GL_COMPILE_STATUS, out var status);
            if (status != GL_FALSE) return;

            glGetShaderiv(handle, GL_INFO_LOG_LENGTH, out var logLength);
            glGetShaderInfoLog(handle, logLength, out _, out var info);
            throw new Exception(info);
        }

        private void CheckProgram(uint handle)
        {
            glGetProgramiv(handle, GL_LINK_STATUS, out var status);
            if (status != GL_FALSE) return;

            glGetProgramiv(handle, GL_INFO_LOG_LENGTH, out var logLength);
            glGetProgramInfoLog(handle, logLength, out _, out var info);
            throw new Exception(info);
        }

        private void CreateFontsTexture()
        {
            _io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height);

            glGetIntegerv(GL_TEXTURE_BINDING_2D, out var lastTexture);
            glGenTextures(1, out _fontTexture);
            glBindTexture(GL_TEXTURE_2D, _fontTexture);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
            _io.Fonts.SetTexID((IntPtr) _fontTexture);
            glBindTexture(GL_TEXTURE_2D, (uint) lastTexture);
        }

        public void Update()
        {
            if (!_io.Fonts.IsBuilt())
                throw new Exception("Font atlas not built !");

            glfwGetWindowSize(_window, out var w, out var h);
            glfwGetFramebufferSize(_window, out var displayW, out var displayH);
            _io.DisplaySize = new Vector2(w, h);
            if (w > 0 && h > 0)
                _io.DisplayFramebufferScale = new Vector2((float) displayW / w, (float) displayH / h);

            var currentTime = glfwGetTime();
            _io.DeltaTime = _time > 0.0 ? (float) (currentTime - _time) : 1.0f / 60.0f;
            _time = currentTime;

            UpdateMousePosAndButtons();
            UpdateMouseCursor();

            ImGui.NewFrame();
        }

        private void UpdateMousePosAndButtons()
        {
            for (var i = 0; i < _io.MouseDown.Count; i++)
            {
                _io.MouseDown[i] = _mouseJustPressed[i] || glfwGetMouseButton(_window, i) != 0;
                _mouseJustPressed[i] = false;
            }

            var mousePosBackup = _io.MousePos;
            _io.MousePos = new Vector2(-float.MaxValue, -float.MaxValue);
            var focused = glfwGetWindowAttrib(_window, GLFW_FOCUSED) != 0;
            if (!focused) return;

            if (_io.WantSetMousePos)
                glfwSetCursorPos(_window, mousePosBackup.X, mousePosBackup.Y);
            else
            {
                glfwGetCursorPos(_window, out var mouseX, out var mouseY);
                _io.MousePos = new Vector2((float) mouseX, (float) mouseY);
            }
        }

        private void UpdateMouseCursor()
        {
            var flag = (_io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0;
            if (flag || glfwGetInputMode(_window, GLFW_CURSOR) == GLFW_CURSOR_DISABLED)
                return;

            var imguiCursor = ImGui.GetMouseCursor();
            if (imguiCursor == ImGuiMouseCursor.None || _io.MouseDrawCursor)
                glfwSetInputMode(_window, GLFW_CURSOR, GLFW_CURSOR_HIDDEN);
            else
            {
                glfwSetCursor(_window,
                    _mouseCursors[(int) imguiCursor] != IntPtr.Zero
                        ? _mouseCursors[(int) imguiCursor]
                        : _mouseCursors[(int) ImGuiMouseCursor.Arrow]);
                glfwSetInputMode(_window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
            }
        }

        public void Render()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0)
                return;

            var fbWidth = (int) (drawData.DisplaySize.X * drawData.FramebufferScale.X);
            var fbHeight = (int) (drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0)
                return;

            glGetIntegerv(GL_ACTIVE_TEXTURE, out var lastActiveTexture);
            glActiveTexture(GL_TEXTURE0);
            glGetIntegerv(GL_CURRENT_PROGRAM, out var lastProgram);
            glGetIntegerv(GL_TEXTURE_BINDING_2D, out var lastTexture);
            glGetIntegerv(GL_SAMPLER_BINDING, out var lastSampler);
            glGetIntegerv(GL_VERTEX_ARRAY_BINDING, out var lastVertexArrayObject);
            glGetIntegerv(GL_ARRAY_BUFFER_BINDING, out var lastArrayBuffer);
            glGetIntegerv(GL_POLYGON_MODE, out var lastPolygonMode);
            glGetIntegerv(GL_VIEWPORT, out var lastViewport);
            glGetIntegerv(GL_SCISSOR_BOX, out var lastScissorBox);
            glGetIntegerv(GL_BLEND_SRC_RGB, out var lastBlendSrcRgb);
            glGetIntegerv(GL_BLEND_DST_RGB, out var lastBlendDstRgb);
            glGetIntegerv(GL_BLEND_SRC_ALPHA, out var lastBlendSrcAlpha);
            glGetIntegerv(GL_BLEND_DST_ALPHA, out var lastBlendDstAlpha);
            glGetIntegerv(GL_BLEND_EQUATION_RGB, out var lastBlendEquationRgb);
            glGetIntegerv(GL_BLEND_EQUATION_ALPHA, out var lastBlendEquationAlpha);

            var lastEnableBlend = glIsEnabled(GL_BLEND);
            var lastEnableCullFace = glIsEnabled(GL_CULL_FACE);
            var lastEnableDepthTest = glIsEnabled(GL_DEPTH_TEST);
            var lastEnableStencilTest = glIsEnabled(GL_STENCIL_TEST);
            var lastEnableScissorTest = glIsEnabled(GL_SCISSOR_TEST);
            var lastEnablePrimitiveRestart = glIsEnabled(GL_PRIMITIVE_RESTART);

            glGenVertexArrays(1, out var vertexArrayObject);
            SetupRenderState(drawData, fbWidth, fbHeight, vertexArrayObject);

            var clipOff = drawData.DisplayPos;
            var clipScale = drawData.FramebufferScale;

            for (var n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                glBufferData(GL_ARRAY_BUFFER, (ulong) (cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()),
                    cmdList.VtxBuffer.Data, GL_STREAM_DRAW);
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, (ulong) cmdList.IdxBuffer.Size * sizeof(ushort),
                    cmdList.IdxBuffer.Data, GL_STREAM_DRAW);

                for (var cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
                {
                    var pcmd = cmdList.CmdBuffer[cmdI];
                    if (pcmd.UserCallback != IntPtr.Zero)
                        throw new NotImplementedException();

                    var clipRect = new Vector4
                    {
                        X = (pcmd.ClipRect.X - clipOff.X) * clipScale.X,
                        Y = (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y,
                        Z = (pcmd.ClipRect.Z - clipOff.X) * clipScale.X,
                        W = (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y
                    };

                    if (!(clipRect.X < fbWidth) || !(clipRect.Y < fbHeight) || !(clipRect.Z >= 0.0f) ||
                        !(clipRect.W >= 0.0f)) continue;

                    glScissor((int) clipRect.X, (int) (fbHeight - clipRect.W), (int) (clipRect.Z - clipRect.X),
                        (int) (clipRect.W - clipRect.Y));

                    glBindTexture(GL_TEXTURE_2D, (uint) pcmd.TextureId);
                    glDrawElementsBaseVertex(GL_TRIANGLES, (int) pcmd.ElemCount, GL_UNSIGNED_SHORT,
                        (IntPtr) (pcmd.IdxOffset * sizeof(ushort)), (int) pcmd.VtxOffset);
                }
            }

            glDeleteVertexArrays(1, vertexArrayObject);

            glUseProgram((uint) lastProgram);
            glBindTexture(GL_TEXTURE_2D, (uint) lastTexture);
            glBindSampler(0, (uint) lastSampler);
            glActiveTexture((uint) lastActiveTexture);
            glBindVertexArray((uint) lastVertexArrayObject);
            glBindBuffer(GL_ARRAY_BUFFER, (uint) lastArrayBuffer);
            glBlendEquationSeparate((uint) lastBlendEquationRgb, (uint) lastBlendEquationAlpha);
            glBlendFuncSeparate((uint) lastBlendSrcRgb, (uint) lastBlendDstRgb, (uint) lastBlendSrcAlpha,
                (uint) lastBlendDstAlpha);
            if (lastEnableBlend) glEnable(GL_BLEND);
            else glDisable(GL_BLEND);
            if (lastEnableCullFace) glEnable(GL_CULL_FACE);
            else glDisable(GL_CULL_FACE);
            if (lastEnableDepthTest) glEnable(GL_DEPTH_TEST);
            else glDisable(GL_DEPTH_TEST);
            if (lastEnableStencilTest) glEnable(GL_STENCIL_TEST);
            else glDisable(GL_STENCIL_TEST);
            if (lastEnableScissorTest) glEnable(GL_SCISSOR_TEST);
            else glDisable(GL_SCISSOR_TEST);
            if (lastEnablePrimitiveRestart) glEnable(GL_PRIMITIVE_RESTART);
            else glDisable(GL_PRIMITIVE_RESTART);
            glPolygonMode(GL_FRONT_AND_BACK, (uint) lastPolygonMode);
            glViewport(lastViewport, lastViewport, lastViewport, lastViewport);
            glScissor(lastScissorBox, lastScissorBox, lastScissorBox, lastScissorBox);
        }

        private void SetupRenderState(ImDrawDataPtr drawData, int fbWidth, int fbHeight, uint vertexArrayObject)
        {
            glEnable(GL_BLEND);
            glBlendEquation(GL_FUNC_ADD);
            glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
            glDisable(GL_CULL_FACE);
            glDisable(GL_DEPTH_TEST);
            glDisable(GL_STENCIL_TEST);
            glEnable(GL_SCISSOR_TEST);
            glDisable(GL_PRIMITIVE_RESTART);
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

            glViewport(0, 0, fbWidth, fbHeight);
            var l = drawData.DisplayPos.X;
            var r = drawData.DisplayPos.X + drawData.DisplaySize.X;
            var t = drawData.DisplayPos.Y;
            var b = drawData.DisplayPos.Y + drawData.DisplaySize.Y;

            var orthoProjection = new[,]
            {
                {2.0f / (r - l), 0.0f, 0.0f, 0.0f},
                {0.0f, 2.0f / (t - b), 0.0f, 0.0f},
                {0.0f, 0.0f, -1.0f, 0.0f},
                {(r + l) / (l - r), (t + b) / (b - t), 0.0f, 1.0f},
            };
            glUseProgram(_shaderHandle);
            glUniform1i(_attribLocationTex, 0);
            glUniformMatrix4fv(_attribLocationProjMtx, 1, false, orthoProjection[0, 0]);

            glBindSampler(0, 0);

            glBindVertexArray(vertexArrayObject);

            glBindBuffer(GL_ARRAY_BUFFER, _vboHandle);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _elementsHandle);
            glEnableVertexAttribArray(_attribLocationVtxPos);
            glEnableVertexAttribArray(_attribLocationVtxUv);
            glEnableVertexAttribArray(_attribLocationVtxColor);

            glVertexAttribPointer(_attribLocationVtxPos, 2, GL_FLOAT, false, Unsafe.SizeOf<ImDrawVert>(),
                Marshal.OffsetOf<ImDrawVert>("pos"));
            glVertexAttribPointer(_attribLocationVtxUv, 2, GL_FLOAT, false, Unsafe.SizeOf<ImDrawVert>(),
                Marshal.OffsetOf<ImDrawVert>("uv"));
            glVertexAttribPointer(_attribLocationVtxColor, 4, GL_UNSIGNED_BYTE, true, Unsafe.SizeOf<ImDrawVert>(),
                Marshal.OffsetOf<ImDrawVert>("col"));
        }

        private void DestroyDeviceObjects()
        {
            if (_vboHandle != 0)
            {
                glDeleteBuffers(1, _vboHandle);
                _vboHandle = 0;
            }

            if (_elementsHandle != 0)
            {
                glDeleteBuffers(1, _elementsHandle);
                _elementsHandle = 0;
            }

            if (_shaderHandle != 0 && _vertHandle != 0)
                glDetachShader(_shaderHandle, _vertHandle);

            if (_shaderHandle != 0 && _fragHandle != 0)
                glDetachShader(_shaderHandle, _fragHandle);

            if (_vertHandle != 0)
            {
                glDeleteShader(_vertHandle);
                _vertHandle = 0;
            }

            if (_fragHandle != 0)
            {
                glDeleteShader(_fragHandle);
                _fragHandle = 0;
            }

            if (_shaderHandle != 0)
            {
                glDeleteProgram(_shaderHandle);
                _shaderHandle = 0;
            }
        }

        private void DestroyFontsTexture()
        {
            if (_fontTexture == 0) return;

            glDeleteTextures(1, _fontTexture);
            _io.Fonts.SetTexID(IntPtr.Zero);
            _fontTexture = 0;
        }

        private void GlfwShutdown()
        {
            for (ImGuiMouseCursor cursorN = 0; cursorN < ImGuiMouseCursor.COUNT; cursorN++)
            {
                glfwDestroyCursor(_mouseCursors[(int) cursorN]);
                _mouseCursors[(int) cursorN] = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            DestroyDeviceObjects();
            DestroyFontsTexture();
            GlfwShutdown();
            ImGui.DestroyContext();
        }
    }
}