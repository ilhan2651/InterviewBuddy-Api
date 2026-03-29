using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.Domain.Enums;

namespace Buddy.Infrastructure.Services.Gemini
{
    public static class PromptMatrix
    {
        private static readonly Random _random = new Random();

        // ─── JUNIOR ───────────────────────────────────────────────────────────────

        private static readonly List<string> _juniorEasy = new()
        {
            "OOP Encapsulation: Property ve field erişim düzeyleri",
            "OOP Inheritance: Base constructor çağrısı ve override mantığı",
            "OOP Polymorphism: Virtual/Override metod hiyerarşisi",
            "OOP Abstraction: Abstract class vs Interface tasarım kıyası",
            "Null Reference Exception: Temel null kontrol stratejileri ve null-coalescing operatörü",
            "HTTP GET vs POST semantik kullanım farkları",
            "RESTful API URI tasarım kuralları (kaynak adlandırma, çoğul vs tekil)",
            "Try-Catch-Finally blok yapısı ve exception yutma (swallowing) riski",
            "Temel SQL: SELECT / WHERE / LIMIT / ORDER BY / GROUP BY kullanımı",
            "Git: Commit, Push, Pull iş akışı ve neden direkt main'e commit tehlikelidir",
            "Git: Branch stratejisi (Feature Branch, Git Flow)",
            "HTTP Durum Kodları: 200/201/204/400/401/403/404/409/500 doğru kullanım senaryoları",
            "Temel Loglama: Console.Log yeterliliğinin sınırları ve Logger hiyerarşisi",
            "Ortam Değişkenleri: Neden .env dosyası git'e gömülmemeli?",
            "Docker nedir ve sanal makineden (VM) ne farkı vardır?",
            "HTML Semantik etiketler (header, section, article, main, nav) ve SEO etkisi",
            "CSS Box Model: Margin/Padding/Border/Content hesaplaması ve box-sizing farkı",
            "JavaScript var vs let vs const ve Block Scope farkları",
            "Array metodları: map, filter, reduce, find, some, every kullanımı",
            "Arrow Functions ve geleneksel fonksiyonlarda 'this' bağlam (context) kaybı",
            // doubles
            "Recursion: Temel özyinelemeli fonksiyon ve Stack Overflow riski",
            "Hata yönetiminde tip güvenliği: Genel Exception yakalamak yerine özel hata türleri",
            "Static vs Instance üyelerin bellek ve yaşam süresi farkı",
            "Temel algoritmik karmaşıklık: O(1) vs O(n) vs O(n²) — basit örneklerle",
            "Interface Segregation temeli: Kullanılmayan metod içeren arayüzün uygulamayı zorlaması",
            "Versiyon kontrol geçmişini bozmamak: git amend ve force push zararları",
            "Temel debugging: Breakpoint, watch, call stack penceresi kullanımı",
            "Yorum (Comment) vs clean code: Kodu açıklayan yorum mu, okunabilir kod mu?",
            "Temel ağ kavramları: DNS, IP, Port ve bir isteğin sunucuya ulaşma yolculuğu",
            "API Key güvenliği: Frontend kodunda API Key gömmek neden tehlikelidir?",
        };

        private static readonly List<string> _juniorMedium = new()
        {
            "Closures: Kapanışların belleği ne zaman serbest bıraktığı ve yan etkileri",
            "Event Bubbling & Capturing: stopPropagation ne zaman doğru bir karardır?",
            "Asenkron JS: Promise zinciri vs async/await'de hata yönetim farkları",
            "Fetch API ile CORS Pre-flight isteği nasıl tetiklenir?",
            "Collections: List<T> vs Dictionary<K,V> sorgulama performans farkı",
            "Veritabanı ilişkileri: 1-N ve N-N JOIN sorgularının yazımı",
            "SQL INDEX: Sorgu hızlanması ile yazma maliyeti dengesini yönetmek",
            "ORM Lazy Loading neden N+1 sorununa yol açar?",
            "Clean Code: Uzun metodları parçalama — tek sorumluluk prensibi uygulaması",
            "DRY Prensibi: Kod tekrarını tespit ve küçük refactor adımları",
            "Git Merge Conflict: Manuel çözüm adımları ve araç seçimi",
            "Git Rebase vs Merge: Farklar ve ne zaman hangisi kullanılır?",
            "JWT Token anatomisi: Header, Payload, Signature içerikleri",
            "Session vs Token tabanlı kimlik doğrulama kıyası",
            "Parola hashing: MD5 neden parola için kullanılmaz? Bcrypt mantığı",
            "Form validasyonu: Neden hem frontend hem backend validasyonu zorunludur?",
            "CSS Specificity: Hangi kural kazanır? Özel katmanlar nasıl hesaplanır?",
            "Flexbox: align-items vs justify-content eksen farkları",
            "Responsive tasarım: Mobile-first yaklaşımı ve medya sorguları",
            "Dockerfile yazımı ve layer cache mekanizması",
            // doubles
            "INNER JOIN vs LEFT JOIN: Hangi durumda NULL satırlar beklenir?",
            "Primary Key vs Unique Constraint: Tasarım kararındaki semantik fark",
            "API Timeout: İstemci ne kadar beklemeli, sunucu ne zaman iptal etmeli?",
            "Magic Number sorunu: Kodda sayısal sabitler neden named constant olmalı?",
            "Dependency injection olmadan unit test yazmak neden zordur?",
            "CSS Grid vs Flexbox: Ne zaman hangisi daha uygun?",
            "Temel rate limiting: Sunucu aşırı istekten korumak için Cookie/IP tabanlı sayaç",
            "Pagination: Offset tabanlı sayfalamada büyük veri setinde performans sorunu",
            "LocalStorage'a JWT token koymak: Hangi saldırıya kapı açar?",
            "Git stash: Yarım işi kaydetmek ve farklı branch'e geçmek",
        };

        private static readonly List<string> _juniorHard = new()
        {
            "Stack vs Heap: Value type ve reference type bellek modeli",
            "Garbage Collection temeli: Ne zaman çalışır, nasıl erken tetiklenmez?",
            "Thread nedir? Main thread'i bloke eden bir sync çağrı senaryosu",
            "async/await: ConfigureAwait(false) ne zaman kritik olur?",
            "JavaScript Event Loop: Microtask vs Macrotask kuyruğu sırası",
            "Race Condition başlangıcı: İki eş zamanlı istek aynı kaydı güncelliyor",
            "SQL Deadlock: Temel bir kilitlenme senaryosunu açıkla ve önlem al",
            "INDEX etkisi: EXPLAIN ANALYZE çıktısı nasıl yorumlanır?",
            "Memory Leak: Event listener kaldırılmadığında ne olur?",
            "XSS: Kullanıcı girdisi DOM'a yazılırken sanitize edilmemesi",
            "Stack Overflow: Sonsuz recursive çağrı senaryosu",
            "CSRF: Form tabanlı saldırı mekanizması ve SameSite Cookie koruması",
            // doubles
            "String interning: Büyük döngüde string birleştirmenin O(n²) bellek etkisi",
            "Thread-safe koleksiyonlar: ConcurrentDictionary ne zaman Dictionary'nin önüne geçmeli?",
            "Connection leak: using bloğu olmayan db connection yaşam döngüsü",
            "SQL N+1 gerçek hayat: ORM ile yüklenen 1000 kayıtın arka planda 1001 sorgu üretmesi",
            "Immutability: Değişmez nesnelerin thread-safety avantajı",
            "Short-circuit evaluation: && ve || operatörlerinin yan etki sıralamasına etkisi",
        };

        // ─── MID ─────────────────────────────────────────────────────────────────

        private static readonly List<string> _midEasy = new()
        {
            "SOLID Single Responsibility: Şişman sınıfı küçük servislerle bölme",
            "SOLID Open/Closed: Yeni özellik için eski kod değiştirilmemeli",
            "SOLID Liskov Substitution: Alt sınıf üst sınıfın yerine kullanılabilmeli",
            "SOLID Interface Segregation: Şişman interface'leri bölme",
            "SOLID Dependency Inversion: Concrete'e değil abstraction'a bağımlılık",
            "Dependency Injection: Constructor vs Property injection senaryoları",
            "Repository Pattern: Doğrudan DbContext kullanmak neden sorunlu?",
            "Unit Test: AAA (Arrange-Act-Assert) prensibi uygulaması",
            "Test Double: Mock vs Stub vs Fake farkları ve doğru seçim",
            "RESTful API versiyonlama stratejileri (URL path, Header, Query Param)",
            "API Rate Limiting: Token Bucket vs Sliding Window algoritması",
            "In-Memory Caching: Her zaman doğru olmayan senaryo örnekleri",
            "Background Jobs: Hangfire vs WorkerService karşılaştırması",
            "Middleware kavramı: İstek pipeline'ına etkisi ve sıralama önemi",
            "Swagger/OpenAPI: API dokümantasyonunun kritikliği ve üretimde kapatma",
            // doubles
            "Factory Method Pattern: Nesne oluşturma kararını çağırandan gizleme",
            "Observer Pattern: Event-based iletişim ve bellek sızıntısı riski",
            "Strategy Pattern: Runtime'da algoritma değiştirme senaryosu",
            "Builder Pattern: Karmaşık nesneyi adım adım oluşturma ve fluent API",
            "Global Exception Handling Middleware: Her endpoint'e try-catch yazmak yerine",
            "Health Check Endpoint: Load balancer için sağlık sinyali vermek",
            "Idempotent REST: PUT vs PATCH anlambilimsel farkı ve tekrar çağrı güvenliği",
            "Feature Flag: Yeni özelliği kapalı deploy edip sonra açmak",
            "Configuration yönetimi: Hardcoded değer vs appsettings vs ortam değişkeni hiyerarşisi",
            "API Response Cache: HTTP Cache-Control header doğru kullanımı",
        };

        private static readonly List<string> _midMedium = new()
        {
            "Redis Cache-Aside Pattern ve veri tutarsızlığı senaryosu",
            "Redis Distributed Lock (Redlock) ile kritik bölge koruması",
            "RabbitMQ Exchange türleri: Direct, Fanout, Topic, Headers karşılaştırması",
            "Message Queue: At-least-once vs exactly-once delivery anlamları ve tasarımı",
            "Veritabanı: Clustered vs Non-Clustered Index karar kriterleri",
            "Connection Pool tüketimi: 'Too many connections' krizi ve pool yönetimi",
            "ORM Eager Loading ile gereksiz JOIN'lerden kaçınma stratejisi",
            "CI/CD Pipeline: Build → Test → Deploy aşamaları ve geri alma stratejisi",
            "Docker Compose: Servisler arası ağ izolasyonu ve volume yönetimi",
            "Kubernetes: Pod, Deployment, Service ilişkisi ve temel scale-out",
            "React useEffect bağımlılık dizisi hataları ve sonsuz render döngüsü",
            "React useMemo vs useCallback: Yeniden render önleme stratejileri",
            "Virtual DOM Reconciliation: Diff algoritması nasıl çalışır?",
            "Webpack Code Splitting ve Lazy Loading stratejileri",
            "PWA Service Worker Cache stratejileri: Cache-First vs Network-First",
            // doubles
            "Outbox Pattern: DB ve mesaj kuyruğu arasında çift yazımı atomik yapma",
            "Saga Choreography vs Orchestration: Hangi senaryo için hangisi?",
            "Dead Letter Queue: İşlenemeyen mesajların izlenmesi ve yeniden deneme politikası",
            "Redis Pub/Sub vs Streams: Kalıcılık ve consumer grup farkı",
            "Soft Delete: Veriyi silmek yerine işaretlemek — sorgu performansı maliyeti",
            "Multi-tenancy: Şema bazlı vs satır bazlı izolasyon kararları",
            "API Gateway vs BFF: Tek giriş noktası ne zaman yük haline gelir?",
            "Database Migration: Geriye dönük uyumlu olmayan değişikliği sıfır kesinti ile uygulama",
            "Distributed Tracing: Correlation ID'yi servisten servise taşıma stratejisi",
            "gRPC: Binary serialization avantajı ve proto sözleşme değişikliği riski",
        };

        private static readonly List<string> _midHard = new()
        {
            "Async Deadlock: ASP.NET Context'te .Result çağrısının kilitleme sebebi",
            "Concurrency: SemaphoreSlim vs lock vs Interlocked farkları",
            "Cache Stampede: Aynı anda binlerce istek boş cache'e çarparsa ne olur?",
            "Cache Invalidation: Veri güncellenince ilişkili tüm cache'leri doğru temizleme",
            "CQRS: Command ve Query modellerinin ayrılmasının getirdiği okuma gecikmesi",
            "Event Sourcing: Mutable state yerine event log'dan state rebuild etme",
            "Distributed Tracing: Servisler arası isteği izleme (Correlation ID propagation)",
            "Idempotency: Aynı mesajın iki kez işlenmesini önleme mekanizması",
            "Frontend Memory Leak: setInterval/setTimeout'u component unmount'ta temizlememek",
            "SSR Hydration Mismatch: Server HTML ile client render çakışması senaryosu",
            // doubles
            "Two-Phase Commit: Dağıtık transaction kesinliğinin performans bedeli",
            "Write-Ahead Log (WAL): Veritabanının çöküş sonrası veri kurtarma mekanizması",
            "Optimistic vs Pessimistic Locking: Çakışma beklentisine göre strateji seçimi",
            "Index Covering Query: Tablo erişimi olmadan sadece indexten sorgu çekme",
            "JSON Column: İlişkisel tabloda yarı yapılandırılmış veri saklama tradeoff'ları",
            "Timeout Cascade: Servis A → B → C zincirinde birikimli gecikme problemi",
            "Circuit Breaker: Sürekli bağlantı denemesinin sistemi boğması — açma/kapama mantığı",
            "Retry Storm: Tüm istemcilerin aynı anda yeniden denemesi ve eksponansiyel backoff",
            "Thread Pool Starvation: async/await yanlış kullanımıyla threadlerin tükenmesi",
            "Hot Partition: Kafka/Shard'da tek anahtara yoğunlaşan yazım darboğazı",
        };

        // ─── SENIOR ───────────────────────────────────────────────────────────────

        private static readonly List<string> _seniorEasy = new()
        {
            "Code Review kültürü: Yapıcı geribildirim vs blokaj — nasıl denge kurulur?",
            "Technical Debt: Ne zaman kabul edilmeli, ne zaman acil ödenmeli?",
            "Junior'a Mentorluk: Spesifik hatayı öğretme — yapboz parçasını vermek",
            "Agile Retrospective: Takımı yavaşlatan sistematik bir engeli ortaya çıkarma",
            "Legacy Refactor: Test yazılmamış bir modülü durdurmadan iyileştirme kararı",
            // doubles
            "Postmortem kültürü: Olay sonrası suçlama yerine sistem iyileştirme odağı",
            "RFC / ADR yazımı: Mimari kararı belgelemek ve ekipten onay almak",
            "Interview tasarımı: Takım için adayı ölçecek teknik mülakat sorusu tasarımı",
            "Onboarding planı: Yeni mühendisi üretken yapmak için 30-60-90 gün çerçevesi",
            "Refactoring safety net: Test olmayan yerde güvenli refactor adımları",
        };

        private static readonly List<string> _seniorMedium = new()
        {
            "Mikroservislere geçiş: Monolitten hangi bounded context ilk ayrılmalı?",
            "Domain Driven Design: Aggregate Root ve domain event tasarım kararları",
            "Saga Pattern: Distributed transaction'da compensating transaction stratejisi",
            "Kafka Consumer Group, Partition ve Offset yönetimi senaryosu",
            "Kafka Rebalancing sırasında mesaj kaybı ve commit stratejisi kararı",
            "API Gateway: Tek giriş noktasının avantajları ve single point of failure riski",
            "Service Mesh (Istio/Linkerd): Sidecar proxy ve mTLS yönetimi",
            "Veritabanı Sharding: Yatay bölümleme stratejisi ve hot shard problemi",
            "Read Replica: Replikasyon gecikmesinin (replication lag) uygulamaya etkisi",
            "GraphQL vs REST: Over-fetching / under-fetching ve N+1 karşılaştırması",
            // doubles
            "Strangler Fig Pattern: Legacy sistemi arka planda yeni mimariyle adım adım değiştirme",
            "Event-Carried State Transfer: Olayların tam state taşıması ve mesaj büyümesi",
            "Backpressure: Tüketici yetişemeyince üreticinin akışını yavaşlatma mekanizması",
            "Anti-Corruption Layer: Dış sistemin veri modelini iç domain'e izole etme",
            "Data Mesh temeli: Domain ekiplerinin kendi veri ürünlerini sahiplenmesi",
            "Multi-region deployment: Kullanıcıya yakın sunma ve veri tutarlılığı gerilimi",
            "Feature Store: ML özelliklerinin birden fazla model ve servis arasında tutarlılığı",
            "Internal Developer Platform: Takımların bağımsız deploy edebilmesi için altyapı soyutlama",
            "API Contract Testing (Pact): Tüketici ve üretici arasında sözleşme güvencesi",
            "Observability üçgeni: Metrics, Logs ve Traces arasındaki boşluk analizi",
        };

        private static readonly List<string> _seniorHard = new()
        {
            "CAP Teoremi: Network partition sırasında Consistency mi Availability mi?",
            "Split-Brain: İki node'un aynı anda Primary olduğunu düşündüğü senaryo",
            "Raft Consensus: Leader election ve log replication adımları",
            "Veritabanı MVCC: Snapshot Isolation ve Write Skew anomalisi",
            "Consistent Hashing: Node eklenince minimum yeniden dağıtım",
            "Node.js Event Loop Tıkanması: CPU-bound iş ve Worker Thread çözümü",
            "Kubernetes OOMKilled: Pod'un bellek limiti neden aşılıyor?",
            "Chaos Engineering: Üretim sisteminde kasıtlı arıza enjeksiyonu kararı",
            "Zero-Downtime DB Migration: Büyük tabloda sütun ekleme / enum değişimi",
            "Distributed Lock: Redis Redlock algoritmasının clock-drift açığı",
            // doubles
            "CRDTs (Conflict-Free Replicated Data Types): Çatışmasız dağıtık veri yapıları",
            "Log Compaction: Kafka'da sonsuz büyüyen topic'i sıkıştırma stratejisi",
            "Phantom Read: Serializable Isolation olmadan aralık sorgularında görüntü yanılması",
            "Thundering Herd: Önbellek sona erince binlerce isteğin aynı anda DB'ye dalması",
            "Backfill Pipeline: Üretim sistemini durdurmadan geçmiş veriye yeni işlem uygulama",
            "Exactly-once Semantics: Kafka Transactions API mekanizması",
            "Long Tail Latency: P99 gecikme optimizasyonu — hedged request stratejisi",
            "Global Secondary Index darboğazı: DynamoDB GSI'nin hot key problemi",
            "Quorum Reads/Writes: DynamoDB / Cassandra'da tutarlılık seviyesi ayarlaması",
            "Zero-copy I/O: sendfile() ve kernel-user space geçişini azaltma",
        };

        // ─── LEAD ────────────────────────────────────────────────────────────────

        private static readonly List<string> _leadEasy = new()
        {
            "Teknik Bütçe: Yeni özellik vs teknik borç öncelik kararı nasıl verilir?",
            "Organizasyonel Anlaşmazlık: Backend ve Frontend takımı API sözleşmesinde uyuşamıyor",
            "Blameless Postmortem: Parmak işaret kültüründen öğrenen organizasyona geçiş",
            "Delivery vs Quality: Sprint baskısına karşı test kalitesini savunma",
            "Onboarding Hızlandırma: Yeni kıdemli mühendisi 2 haftada üretken kılma planı",
            // doubles
            "Mühendis motivasyonu: Teknik çalışma vs ürün talebi dengesizliği nasıl yönetilir?",
            "Yetenek kaybı: Kritik bir Senior'ın ayrılmasında bilgi transferi acil planı",
            "Mühendislik ilkeleri: Tüm ekipler için ortak kodlama standartları nasıl benimsenir?",
            "Sessiz çoğunluk: Retro'da hiç konuşmayan mühendisleri sürece dahil etmek",
            "OKR hizalaması: Teknik yatırımı iş hedeflerine bağlamak",
        };

        private static readonly List<string> _leadMedium = new()
        {
            "Platform Ekibi: Ortak kütüphane kararını 5 ayrı takıma nasıl uygularsın?",
            "Teknoloji Seçimi: Yeni servis dili seçerken ekosistem değerlendirme çerçevesi",
            "Breaking Change Yönetimi: Geriye dönük uyumsuz API değişikliğini planlama",
            "SLA/SLO Tanımı: Uptime hedefini ürün ve iş birimleriyle müzakere etme",
            "Vendor Lock-in: Üçüncü parti API bırakıldığında acil migrasyon planı",
            // doubles
            "Architecture Decision Record (ADR): Kararların neden ve nasıl belgeleneceği",
            "Conway's Law: Takım yapısının mimariyi şekillendirmesi ve buna karşı koymak",
            "FinOps kültürü: Mühendislere bulut maliyetini sahiplendirme",
            "Çapraz takım bağımlılık: İki takım aynı servise bağımlıyken nasıl bağımsız deploy edilir?",
            "Teknik röportaj süreci: Şirketin ihtiyaçlarını yansıtan adil bir mülakat tasarımı",
        };

        private static readonly List<string> _leadHard = new()
        {
            "Büyük Ölçekli Göç: 10 mikroservisi etkileyen veri modeli değişikliğini sıfır kesinti ile uygulama",
            "Cloud Maliyet Krizi: Aylık fatura %400 arttı — darboğaz tespiti ve acil eylem planı",
            "Güvenlik İhlali: Sistemde yetkisiz erişim tespit edildi — müdahale zaman çizelgesi",
            "Polyglot Persistence: Monolitik DB'den çok veritabanı mimarisine geçiş stratejisi",
            "Şirket Çapında Performans Krizi: Hangi ekip, hangi araç, hangi öncelik sırası?",
            // doubles
            "Regulatory Compliance: GDPR/KVKK kapsamı genişledi — sistemin 6 ayda uyum planı",
            "M&A entegrasyonu: Satın alınan şirketin altyapısını ana sistemle birleştirme stratejisi",
            "Mühendislik verimliliği: DORA metrikleri ile build pipeline'ını iyileştirme planı",
            "Kapsamlı güvenlik denetimi: Pentest sonuçlarını ekiplere aktarma ve önceliklendirme",
            "Stratejik yeniden yazım (Rewrite): Big bang yerine aşamalı geçişin tehlikeli kararı",
        };

        // ─── Domain Extras ────────────────────────────────────────────────────────

        private static readonly Dictionary<string, Dictionary<InterviewLevel, List<string>>> _domainExtras = new()
        {
            ["frontend"] = new()
            {
                [InterviewLevel.Junior] = new() { "CSS Grid: Karmaşık layout için doğru breakpoint stratejisi", "Event Delegation: Her li'ye ayrı listener yerine parent'a tek listener avantajı", "FOUC (Flash of Unstyled Content) neden oluşur ve nasıl önlenir?", "Lazy Image Loading: <img loading='lazy'> ve Intersection Observer karşılaştırması", "useRef: DOM referansı için useState yerine neden useRef?" },
                [InterviewLevel.Mid]   = new() { "React Suspense + lazy: Bundle bölme ve yükleme durumu yönetimi", "useMemo her yerde: Neden performansı artırmak yerine düşürür?", "CSS-in-JS: Runtime maliyet vs build-time şablonlama kıyası", "React.memo ile yanlış kullanım: Referans eşitliği tuzağı", "Tarayıcı cache: ETag ve Last-Modified ile koşullu HTTP isteği" },
                [InterviewLevel.Senior]= new() { "Micro-Frontend: Module Federation ile iki ekibin bağımsız deploy etmesi", "Core Web Vitals bütçesi: LCP > 2,5s olan sayfayı adım adım iyileştirme", "V8 JIT Deoptimization: Monomorphic vs polymorphic call site", "Islands Architecture: Kısmi hidrasyon ile SSR performansı", "WASM entegrasyonu: CPU-yoğun işlemi JavaScript'ten WebAssembly'ye taşımak" },
                [InterviewLevel.Lead]  = new() { "Design System çok takımlı şirkete yayma stratejisi", "Framework Migration: Angular → React geçişini sıfır kesinti ile planlama", "Accessibility (A11y) bütçesi: Tüm ürün ekranlarını WCAG 2.1 AA'ya taşımak" },
            },
            ["backend"] = new()
            {
                [InterviewLevel.Junior] = new() { "Dependency Injection neden new Foo() yerine tercih edilir?", "Temel SQL Transaction: BEGIN/COMMIT/ROLLBACK ne zaman gerekli?", "API Response şablonu: Tutarsız JSON yapısının istemciyi nasıl bozduğu", "Repository soyutlaması: Neden testi kolaylaştırır?", "Async void: Neden exception yakalanamaz ve ne zaman tehlikelidir?" },
                [InterviewLevel.Mid]   = new() { "Redis Pub/Sub: Ne zaman Kafka yerine tercih edilebilir?", "Outbox Pattern: Veritabanı ve mesaj kuyruğu arasında atomik yazım", "API Pagination: Cursor tabanlı sayfalama neden offset'in önüne geçer?", "Idempotency Key: REST API'da tekrar eden isteği güvenle işlemek", "Optimistic Concurrency: rowversion/ETag ile kayıp güncelleme önleme" },
                [InterviewLevel.Senior]= new() { "Sidecar Pattern: Loglama ve güvenliği ana koddan ayırma", "Database per Service: Servisler arası join sorunu ve olay odaklı çözüm", "gRPC vs REST: Servisler arası iletişimde bant genişliği ve gecikme kıyası", "Bulkhead Pattern: Yavaş servisin tüm thread pool'u tüketmesini engelleme", "Event-carried State Transfer: Downstream için tam state mi, değişim delta mı?" },
                [InterviewLevel.Lead]  = new() { "Conway's Law: Organizasyon yapısının API sözleşmelerini şekillendirmesi", "API Economy: Dış geliştiricilere sunulan API'ın versiyonlama ve deprecation politikası", "Platform as a Product: Altyapı ekibinin iç müşteriye hizmet tasarımı" },
            },
            ["mobile"] = new()
            {
                [InterviewLevel.Junior] = new() { "Ekran Döndürme: State kaybını önleme stratejisi", "Runtime Permission başarısız olduğunda graceful degradation" },
                [InterviewLevel.Mid]   = new() { "Offline-First: Yerel veri ve sunucu senkronizasyon çakışması", "Deep Link: Schema URL vs App Link güvenlik farkları", "Pil optimizasyonu: Arka plan görevinin doğru zamanlanması" },
                [InterviewLevel.Senior]= new() { "iOS ARC Retain Cycle: Closure içinde weak self gereği", "Gradle/Xcode Build Optimizasyonu: Büyük projede derleme süresini kısaltma", "Modüler mimari: Feature module bağımsız build ve test süreci" },
                [InterviewLevel.Lead]  = new() { "Flutter vs React Native vs Native: Uzun vadeli organizasyonel maliyet analizi", "App Store CI/CD: Tam otomasyon ve Beta dağıtım politikası" },
            },
            ["devops"] = new()
            {
                [InterviewLevel.Junior] = new() { "SSH key-pair kurulumu: Parolasız bağlantı güvenliği", "Linux chmod 777 neden tehlikelidir?", "Container log: Stdout'a yazmanın sağladığı taşınabilirlik" },
                [InterviewLevel.Mid]   = new() { "Terraform Remote State: Lock mekanizması neden kritik?", "K8s Liveness vs Readiness Probe yanlış yapılandırmanın cascade etkisi", "Helm Chart: Yeniden kullanılabilir uygulama paketi ve release yönetimi" },
                [InterviewLevel.Senior]= new() { "Blue/Green Deployment: DNS cutover sırasında eski ortamın bekletme süresi", "Chaos Engineering: Servisi kontrollü kapatıp sistem tepkisini gözlemleme", "Service Mesh mTLS: Servisler arası şifreli iletişim ve sertifika rotasyonu" },
                [InterviewLevel.Lead]  = new() { "FinOps: Bulut harcamayı görünür kılmak ve mühendisleri maliyet sahipliğine dahil etmek", "Multi-cloud strateji: Vendor bağımlılığını azaltma vs operasyonel karmaşıklık artışı" },
            },
            ["data"] = new()
            {
                [InterviewLevel.Junior] = new() { "Train/Test sızıntısı: Feature engineering sırasında bilginin test setine kaçması", "Sınıf dengesizliği: %0,1 fraud oranında accuracy metriğinin yanıltıcılığı" },
                [InterviewLevel.Mid]   = new() { "MLflow: Model versiyonlama ve deney takibi stratejileri", "Feature Store: Aynı feature'ın tutarlı tutulması farklı modellerde" },
                [InterviewLevel.Senior]= new() { "Data Drift: Üretim verisinin eğitim setinden kayması ve model izleme", "Streaming ML: Gerçek zamanlı öneri sisteminde Flink vs Spark karşılaştırması" },
                [InterviewLevel.Lead]  = new() { "Data Mesh: Dağıtık veri sahipliği modeli ve domain ekipler arasında veri ürün standardı" },
            },
            ["security"] = new()
            {
                [InterviewLevel.Junior] = new() { "SQL Injection: Parametreli sorgu ile karakter filtrelemesinin farkı", "Cookie bayrakları: HttpOnly, Secure, SameSite session hijacking'e etkisi" },
                [InterviewLevel.Mid]   = new() { "SSRF vs CSRF: Mimari fark ve her birinin sunucu üzerindeki etki alanı", "JWT alg:none zafiyeti: İmzasız token kabul eden sistemin açığı" },
                [InterviewLevel.Senior]= new() { "Threat Modeling: STRIDE ile yeni özelliğin saldırı yüzeyi analizi", "WAF Bypass: Encoding teknikleriyle kural motorunu atlatma senaryosu" },
                [InterviewLevel.Lead]  = new() { "GDPR/KVKK Uyum: Veri minimizasyonu prensibini mühendislik süreçlerine entegre etme" },
            },
        };

        // ─── ENTRY POINT ─────────────────────────────────────────────────────────

        public static string GetGuidance(string profession, string jobTitle, InterviewLevel level, DifficultyLevel difficulty)
        {
            var sharedPool   = GetSharedPool(level, difficulty);
            var domainExtras = GetDomainExtras(profession, jobTitle, level);
            var combined     = sharedPool.Concat(domainExtras).ToList();

            int pick = difficulty == DifficultyLevel.Hard ? 3 : 2;
            var selected = combined.OrderBy(_ => _random.Next()).Take(pick);

            return
                "ÖRNEK ODAK DİSİPLİNLERİ (rastgele seçildi—bunları ilham olarak kullan):\n" +
                string.Join("\n", selected.Select((t, i) => $"  {i + 1}. {t}")) +
                "\n\n" +
                "KURALLAR:\n" +
                "  • Bu disiplinleri sadece bir BAŞLANGIÇ REFERANSı olarak kullan. Bunlara birebir bağlı kalma.\n" +
                "  • Kendi yaratıcılığınla yeni, ilgili veya tamamen farklı teknik krizler, senaryolar ve vakalar üretebilirsin.\n" +
                "  • Her soru unique olmalı; aynı konseptleri tekrar eden ya da şablon gibi görünen sorular KABUL EDİLMEZ.\n" +
                "  • Asla 'Nedir?', 'Açıkla?', 'Avantajları neler?' gibi basit tanım soruları SORMA.\n" +
                "  • Adayı gerçek bir kriz, mimari karar veya kod senaryosunun içine at.";
        }

        // ─── ROUTERS ─────────────────────────────────────────────────────────────

        private static List<string> GetSharedPool(InterviewLevel level, DifficultyLevel difficulty) =>
            (level, difficulty) switch
            {
                (InterviewLevel.Junior, DifficultyLevel.Easy)   => _juniorEasy,
                (InterviewLevel.Junior, DifficultyLevel.Medium) => _juniorMedium,
                (InterviewLevel.Junior, DifficultyLevel.Hard)   => _juniorHard,
                (InterviewLevel.Mid,    DifficultyLevel.Easy)   => _midEasy,
                (InterviewLevel.Mid,    DifficultyLevel.Medium) => _midMedium,
                (InterviewLevel.Mid,    DifficultyLevel.Hard)   => _midHard,
                (InterviewLevel.Senior, DifficultyLevel.Easy)   => _seniorEasy,
                (InterviewLevel.Senior, DifficultyLevel.Medium) => _seniorMedium,
                (InterviewLevel.Senior, DifficultyLevel.Hard)   => _seniorHard,
                (InterviewLevel.Lead,   DifficultyLevel.Easy)   => _leadEasy,
                (InterviewLevel.Lead,   DifficultyLevel.Medium) => _leadMedium,
                (InterviewLevel.Lead,   DifficultyLevel.Hard)   => _leadHard,
                _ => _juniorMedium
            };

        private static List<string> GetDomainExtras(string profession, string jobTitle, InterviewLevel level)
        {
            var p  = (profession ?? "").ToLowerInvariant();
            var j  = (jobTitle   ?? "").ToLowerInvariant();
            string key = "";

            if (j.Contains("frontend") || j.Contains("react") || j.Contains("angular") || j.Contains("vue"))
                key = "frontend";
            else if (j.Contains("backend") || j.Contains("api") || j.Contains("node") || j.Contains("core"))
                key = "backend";
            else if (j.Contains("full stack") || j.Contains("fullstack"))
                key = "backend"; // fullstack uses backend extras
            else if (j.Contains("mobile") || j.Contains("ios") || j.Contains("android") || j.Contains("flutter"))
                key = "mobile";
            else if (j.Contains("devops") || j.Contains("sre") || p.Contains("sistem"))
                key = "devops";
            else if (p.Contains("veri") || p.Contains("data") || p.Contains("yapay"))
                key = "data";
            else if (p.Contains("siber") || p.Contains("güvenlik") || p.Contains("security"))
                key = "security";

            if (key != "" && _domainExtras.TryGetValue(key, out var byLevel) && byLevel.TryGetValue(level, out var extras))
                return extras;

            return new List<string>();
        }
    }
}
