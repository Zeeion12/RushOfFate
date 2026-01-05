# 🔄 Auto-Reset System - Panduan

## ✅ Apa yang Sudah Dibuat?

Sistem auto-reset yang akan **otomatis menghapus semua progress** ketika:
1. **Pertama kali buka build** (fresh install)
2. **Versi build berbeda** dari versi terakhir yang dimainkan

## 📁 File yang Dibuat/Diubah:

1. **BuildVersionManager.cs** (BARU)
   - Lokasi: `Assets/Script/Manager/BuildVersionManager.cs`
   - Fungsi: Deteksi build baru dan auto-reset semua data

2. **LogoController.cs** (DIUBAH)
   - Lokasi: `Assets/Script/CutScene/LogoController.cs`
   - Perubahan: Tambah inisialisasi BuildVersionManager di Start()

## 🧪 Cara Testing:

### Testing 1: Build Pertama Kali
1. **Buka game di Editor**, main sesukanya, unlock beberapa level
2. **Build game** (File → Build Settings → Build)
3. **Jalankan build** yang baru dibuat
4. **HASIL YANG DIHARAPKAN:**
   - Semua progress ter-reset
   - Hanya Tutorial yang unlocked
   - Inventory kosong
   - Log console: "New build detected! Resetting all progress..."

### Testing 2: Build yang Sama (Tidak Reset)
1. **Jalankan build** yang sudah pernah dibuka
2. **Main** dan unlock beberapa level
3. **Tutup** dan **buka lagi** build yang SAMA
4. **HASIL YANG DIHARAPKAN:**
   - Progress TIDAK ter-reset
   - Level yang sudah di-unlock masih terbuka
   - Log console: "Same build version, keeping progress."

### Testing 3: Update Versi (Reset Lagi)
1. Di Unity Editor, buka **Edit → Project Settings → Player**
2. Ubah **Version** (misal dari 1.0 ke 1.1)
3. **Build ulang** game
4. **Jalankan build** yang baru
5. **HASIL YANG DIHARAPKAN:**
   - Progress ter-reset lagi (karena versi berbeda)
   - Log console: "New build detected! Resetting all progress..."

## 🔍 Debugging:

### Cek Versi di Console:
Build akan menampilkan log seperti ini:
```
[BuildVersion] Current: 1.0, Last: , First Run: True
[BuildVersion] New build detected! Resetting all progress...
[BuildVersion] Progress reset complete! New version saved: 1.0
```

### Jika Auto-Reset TIDAK Berjalan:

1. **Cek apakah BuildVersionManager ada di scene:**
   - Buka LogoScene
   - Play mode
   - Lihat Hierarchy, harusnya ada GameObject "BuildVersionManager"

2. **Cek Log Console:**
   - Cari "[BuildVersion]" di console
   - Harusnya muncul log initialization

3. **Manual Reset:**
   - Tutup game build
   - Jalankan command ini di Command Prompt (Admin):
     ```
     reg delete "HKCU\Software\DefaultCompany\RushOfFate" /f
     ```
   - Buka lagi build

## ⚙️ Cara Ubah Project Version:

1. **Edit → Project Settings**
2. **Player** tab
3. Cari bagian **Version**
4. Ubah angka versi (misal: 1.0 → 1.1 → 2.0)
5. Build ulang

## 🎯 Catatan Penting:

- ✅ Auto-reset **HANYA di BUILD**, tidak di Editor
- ✅ Di Editor, progress tetap tersimpan (tidak ter-reset otomatis)
- ✅ Setiap kali ganti versi dan build → auto-reset
- ✅ Jika buka build yang sama → progress tetap ada

## 🚨 Troubleshooting:

### "Progress masih ada padahal build baru!"
- Pastikan versi berbeda atau PlayerPrefs key `LastBuildVersion` kosong
- Coba manual reset dengan command di atas

### "Di Editor malah ke-reset!"
- Tidak mungkin, auto-reset HANYA di build (`#if !UNITY_EDITOR`)
- Jika di Editor ter-reset, berarti ada script lain yang memanggil reset

### "Ingin disable auto-reset sementara?"
- Buka `BuildVersionManager.cs`
- Comment bagian `#if !UNITY_EDITOR` (line 56)

---

