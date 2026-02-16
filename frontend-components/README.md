# 📦 Frontend Component'leri - Kurulum Guide

## ✅ Oluşturulan Dosyalar

Tüm component'ler `StudyBuddy/frontend-components/` klasöründe hazır!

### 🎨 UI Component'leri
- ✅ **Button.jsx** - 5 varyant (primary, secondary, outline, ghost, danger)
- ✅ **Card.jsx** - Glassmorphism kart component'i
- ✅ **Input.jsx** - İkon ve hata desteği olan input
- ✅ **Progress.jsx** - Animasyonlu progress bar

### 📄 Sayfalar
- ✅ **Login.jsx** - Giriş sayfası (glassmorphism + animasyon)
- ✅ **Register.jsx** - 2-adımlı kayıt sayfası
- ✅ **Dashboard.jsx** - 3-kolonlu dashboard (mülakatlar, stats, CTA)
- ✅ **InterviewSetup.jsx** - Rol seçimi, seviye slider, mikrofon testi
- ✅ **InterviewRoom.jsx** - Unity avatar + ses kaydı + waveform
- ✅ **FeedbackReport.jsx** - Animasyonlu skorlar + accordion

### 🔧 Yardımcı Dosyalar
- ✅ **api.js** - Axios service layer (JWT auth dahil)
- ✅ **App.jsx** - React Router + Protected Routes
- ✅ **tailwind.config.js** - Özel renk paleti
- ✅ **index.css** - Global stiller + custom scrollbar
- ✅ **package.json** - Tüm dependency'ler

---

## 🚀 Kurulum Adımları

### 1. Vite Projesi Oluştur
```bash
cd StudyBuddy
npm create vite@latest frontend -- --template react
cd frontend
```

### 2. Dependency'leri Kur
```bash
npm install
npm install react-router-dom axios lucide-react framer-motion
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### 3. Dosyaları Kopyala

**Klasör yapısı:**
```
frontend/
├── src/
│   ├── components/
│   │   └── ui/
│   │       ├── Button.jsx    (kopyala)
│   │       ├── Card.jsx      (kopyala)
│   │       ├── Input.jsx     (kopyala)
│   │       └── Progress.jsx  (kopyala)
│   ├── pages/
│   │   ├── Login.jsx            (kopyala)
│   │   ├── Register.jsx         (kopyala)
│   │   ├── Dashboard.jsx        (kopyala)
│   │   ├── InterviewSetup.jsx   (kopyala)
│   │   ├── InterviewRoom.jsx    (kopyala)
│   │   └── FeedbackReport.jsx   (kopyala)
│   ├── services/
│   │   └── api.js               (kopyala)
│   ├── App.jsx                  (üzerine yaz)
│   └── index.css                (üzerine yaz)
├── tailwind.config.js           (üzerine yaz)
└── package.json                 (dependency'leri ekle)
```

### 4. Çalıştır
```bash
npm run dev
```

---

## 🎯 Backend API Entegrasyonu

`src/services/api.js` dosyasında base URL var:
```javascript
const API_URL = 'http://localhost:5000/api';
```

**Gerekli Endpoint'ler (Backend'de olmalı):**
- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/interview/start`
- `GET /api/interview/{sessionId}/current-question`
- `POST /api/interview/submit-answer`
- `GET /api/interview/{sessionId}/report`
- `GET /api/user/stats`
- `GET /api/user/recent-interviews`

---

## 🎨 Tasarım Özellikleri

### Renk Paleti
- **Primary:** `#A8E6CF` (Nane yeşili)
- **Secondary:** `#DCD6F7` (Lavanta)
- **Dark:** `#1A1A2E` (Koyu deniz mavisi)

### Özellikler
- ✨ Glassmorphism efektleri
- 🎭 Smooth animasyonlar
- 📱 Responsive tasarım
- 🎨 Custom scrollbar
- 🔒 Protected routes (JWT)

---

## 🎮 Unity Entegrasyonu (Sonraki Adım)

`InterviewRoom.jsx` içinde Unity placeholder var (🤖 emoji).

**Unity WebGL build'i eklemek için:**
1. Unity'den WebGL build al
2. `public/unity/Build/` klasörüne koy
3. `react-unity-webgl` paketini kur:
   ```bash
   npm install react-unity-webgl
   ```
4. `InterviewRoom.jsx` içindeki placeholder'ı Unity component ile değiştir

---

## 📝 Notlar

- Tüm component'ler **Tailwind CSS** ile stillendirilmiş
- **Lucide React** icon'ları kullanılmış
- **Framer Motion** animasyonlar için hazır (şu an kullanılmamış, eklenebilir)
- Ses kaydı **Web Audio API** ile çalışıyor
- JWT token **localStorage**'da tutuluyor

---

## 🐛 Sorun Giderme

**"Module not found" hatası:**
```bash
npm install
```

**Tailwind çalışmıyor:**
```bash
npx tailwindcss init -p
# tailwind.config.js dosyasını kontrol et
```

**API bağlanamıyor:**
- Backend'in çalıştığından emin ol (`http://localhost:5000`)
- CORS ayarlarını kontrol et

---

Tüm component'ler hazır! Kopyala-yapıştır yapabilirsin 🚀
