#XOR shellcode cryptor

key = 0x57

with open("shellcode.bin", "rb") as f:
    shellcode = bytearray(f.read())
encrypted = bytearray()
for b in shellcode:
    encrypted.append(b ^ key)
with open("payload.enc", "wb") as f:
    f.write(encrypted)
