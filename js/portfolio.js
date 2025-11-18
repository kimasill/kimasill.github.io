(function($){
  $(function(){
    // Theme toggle
    var theme = localStorage.getItem('theme') || 'dark';
    $('body').attr('data-theme', theme);
    $('#theme-toggle').on('click', function(){
      var t = $('body').attr('data-theme') === 'dark' ? 'light' : 'dark';
      $('body').attr('data-theme', t);
      localStorage.setItem('theme', t);
    });

    // i18n
    var dict = {
      ko: {
        'nav.home':'Home','nav.work':'Work','nav.about':'About','nav.contact':'Contact',
        'hero.title':'KIMASILL','hero.subtitle':'Game Developer / Programmer',
        'work.title':'Featured Work','filter.all':'All','filter.tools':'Tools','common.details':'자세히',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'About',
        'about.p1':'저는 25년 이상 게임을 만들어온 레벨/시스템 디자이너이자 아티스트, 협업자, 라이터이자 스토리텔러입니다.',
        'about.p2':'우리 문화에 영향을 주는 세계와 경험을 만드는 것이 제 열정입니다.',
        'about.p3':'Half-Life: Alyx에서 플레이어가 시티 17의 거리를 직접 걷게 했고, BioShock 시리즈에서 랩처를 생생하게 만들었습니다. Dance Central에서는 디자인 리드를 맡아 모두를 춤추게 했고, 2인 인디 팀으로 Captain Forever Remix에서 어린 시절 상상력의 즐거움을 불러일으켰습니다.',
        'about.p4':'PC, 콘솔, iOS(터치), Xbox Kinect, VR 등 다양한 장르/플랫폼에서 인터페이스에 맞춘 핸즈온 디자인 경험을 쌓았습니다.',
        'about.p5':'다채롭고 보람 있는 커리어를 이어온 것은 큰 특권이었고, 새로운 기술을 배우고 신선한 크리에이티브 도전을 항상 갈망합니다.',
        'about.cta':'이력서 다운로드'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.about':'About','nav.contact':'Contact',
        'hero.title':'KIMASILL','hero.subtitle':'Game Developer / Programmer',
        'work.title':'Featured Work','filter.all':'All','filter.tools':'Tools','common.details':'Details',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'About',
        'about.p1':'I’m a level and systems designer, artist, creative collaborator, writer and storyteller with 25+ years experience making games.',
        'about.p2':'Creating worlds and experiences that impact our culture is my passion.',
        'about.p3':'I coaxed players to literally step into the streets of City 17 in the groundbreaking VR title Half-Life: Alyx, brought Rapture to life in the critically acclaimed BioShock series, and got the world dancing as design lead on Dance Central. As part of a two-person indie team, I sparked the joy of childhood imagination and creativity in Captain Forever Remix.',
        'about.p4':'My work spans a wide range of genres and platforms—including PC, consoles, iOS (touchscreen), Xbox Kinect, and VR—with hands-on design experience for a diverse set of interfaces.',
        'about.p5':'It’s been a privilege to build a varied and rewarding career, and I’m always eager to learn new skills and take on fresh creative challenges.',
        'about.cta':'Download Resume'
      }
    };
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        var key = $(this).data('i18n');
        if(dict[lang] && dict[lang][key]){
          $(this).text(dict[lang][key]);
        }
      });
      $('#lang-toggle').text(lang==='en'?'EN':'KR');
    }
    var lang = localStorage.getItem('lang') || 'ko';
    applyI18n(lang);
    $('#lang-toggle').on('click', function(){
      lang = (lang==='ko'?'en':'ko');
      localStorage.setItem('lang', lang);
      applyI18n(lang);
    });

    // Filter buttons
    $('.filter-btn').on('click', function(){
      var filter = $(this).data('filter');
      $('.filter-btn').removeClass('active');
      $(this).addClass('active');
      if(!filter || filter === 'all'){
        $('.work-card').show();
        return;
      }
      var f = String(filter).toLowerCase();
      $('.work-card').each(function(){
        var tags = ($(this).data('tags') || '').toString().toLowerCase();
        if(tags.indexOf(f) !== -1){ $(this).show(); } else { $(this).hide(); }
      });
    });

    // IntersectionObserver for reveal and lazy video
    var io = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if(entry.isIntersecting){
          $(entry.target).addClass('in-view');
          var video = $(entry.target).find('video.work-video')[0];
          if(video && !video.src && video.dataset.src){
            video.src = video.dataset.src;
          }
        }
      });
    }, {threshold: 0.2});
    $('.reveal').each(function(){ io.observe(this); });

    // Hover play/pause on videos
    $(document).on('mouseenter', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.play().catch(function(){}); }
    });
    $(document).on('mouseleave', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.pause(); v.currentTime = 0; }
    });
  });
})(jQuery);
