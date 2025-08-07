# SematicKernelWeb
# Semantic Kernel Web is a web application that provides a user interface for interacting with the Semantic Kernel, a framework for building AI applications.
# It allows users to create, manage, and execute AI workflows using various AI models and tools.


# Getting Started
# To get started with Semantic Kernel Web, follow these steps:
# 1. Clone the repository:
# 2. Set up a SearXNG server.  This is linux based but can be run in docker.  I found a good article of how to set it up at https://docs.openwebui.com/tutorials/web-search/searxng  It's important that you follow the directions because if you don't it won't return json which is required for the web search plugin.
# 3. Download Ollama and install it.
# 4. Start Ollama
# 5. Open a command prompt and type in "ollama pull granite3-dense:8b"
# 6. Once that finishes type in "ollama pull PetrosStav/gemma3-tools:12b"
# 7. Ok, next step is to install PostgreSQL.  I used the latest version which is 16.2.  You can find it at https://www.postgresql.org/download/windows/
# 8. Make sure if your not using the docker build that you add in vector support.  You can find the instructions at https://www.postgresql.org/docs/current/pgvector.html
# 9. Create a database called "semantic_kernel_web" and a user called "semantic_kernel_web" with the password "semantic_kernel_web".
# 10. Open the project in Visual Studio or your preferred IDE.
# 11. Restore the NuGet packages.
# 12. Download Comfy at https://github.com/comfyanonymous/ComfyUI/releases/latest/download/ComfyUI_windows_portable_nvidia.7z
# 12a. Download the Checkpoint model at https://huggingface.co/stable-diffusion-v1-5/stable-diffusion-v1-5/blob/main/v1-5-pruned-emaonly.safetensors
# 13. Open the folder you installed Comfy in and go to "Models\Checkpoints" and copy "v1-5-pruned-emaonly.safetensors" into it.  This is the Model I use to generate images.
# 14. Start Comfy, if you have a gpu you can run "run_nvidia_gpu_fast_fp16_accumulation.bat" else you can run "run_cpu.bat".  If you have a gpu and want to use it, make sure you have the latest NVIDIA drivers installed.
# 15. Update the config.json file with your PostgreSQL connection string,SearXNG server URL, comfy information and Ollama Url.
# 16. Run the application.
