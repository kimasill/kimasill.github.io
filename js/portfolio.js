(function($){
  $(function(){
    /* scripts.min.js가 모든 header a에 preventDefault + $(href) 스크롤을 거는데,
       href가 projects/... 또는 dawnstar.html 등이면 선택자가 무효라 네비가 죽음 — proj-header만 일반 링크 동작 */
    $('header.proj-header a').off('click');

    // Theme toggle
    var theme = localStorage.getItem('theme') || 'light';
    $('body').attr('data-theme', theme);
    $('#theme-toggle').on('click', function(){
      var t = $('body').attr('data-theme') === 'dark' ? 'light' : 'dark';
      $('body').attr('data-theme', t);
      localStorage.setItem('theme', t);
    });

    // i18n
    var dict = {
      ko: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'포트폴리오 페이지','common.process':'개발 프로세스 페이지',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'저는 게임 개발자이자 레벨 및 시스템 디자이너, 스토리텔러로서 다년간의 게임 개발 경험을 비롯한 시스템 제작 경험을 가지고 있으며, 수백 가지 이상의 게임을 즐겨 온 게이머이기도 합니다.',
        'about.p2':'저는 사람들에게 깊게 기억되며 마음을 움직이는 경험을 주려고 노력하며, 그것을 창조해 내는 것에 소질이 있고 제 전문 분야입니다. 어릴 적 봤던 히가시노 게이고의 소설들은 내러티브를 어떻게 전달하는가에 대한 확고한 답을 줬고, SF 소설들—특히 류츠신 \'삼체\'를 접하며 상상력은 더 커지고, 단단해졌습니다.',
        'about.p3':'저는 게임을 비롯한 레벨 생성 퀄리티를 상승시키기 위해 최신 기술 PCG를 활용해 절차적 레벨 생성 모듈을 구현해냈고, 데디케이티드 서버 구조를 가진 2D MMORPG의 매끄러운 네트워크 환경을 구축했으며, 전통적인 게임 로직에 머물지 않고, 딥러닝 분류 모델을 개발하며 데이터와 AI가 만들어낼 새로운 가능성을 탐구하기도 했습니다.',
        'about.p4':'제 작업은 언리얼 엔진(Unreal Engine), 유니티(Unity)와 같은 상용 엔진에 한정되지 않습니다. Java, C#의 라이브러리만으로도 생동감 넘치는 게임을 만들어 낸 경험이 있으며 어떠한 환경에도 도전하고 성공해낼 자신이 있습니다.',
        'about.p5':'다양한 사람들과 협업하고, 끊임없이 배우고 도전해 왔습니다. 앞으로도 기꺼이 변화를 받아들이고 새로운 경험을 찾아 모험을 떠날 것입니다.',
        'about.cta':'이력서 다운로드'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'Portfolio page','common.process':'Development process',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'I am a game developer, level and systems designer, and storyteller with years of experience building games and systems—and a gamer who has enjoyed hundreds of titles.',
        'about.p2':'I strive to give people experiences that stay with them and move them; creating those experiences is both my strength and my focus. Keigo Higashino\'s novels showed me early how narrative can be delivered with clarity; science fiction—especially Liu Cixin\'s Three-Body Problem—expanded and solidified my imagination.',
        'about.p3':'To raise the quality of level creation—including in games—I have leveraged modern PCG to deliver procedural level modules, built smooth networked play for a 2D MMORPG on a dedicated-server architecture, and ventured beyond classic game logic into deep learning classifiers to explore what data and AI can unlock.',
        'about.p4':'My work is not limited to commercial engines like Unreal Engine or Unity. I have shipped lively games using only Java and C# libraries, and I am confident I can take on and succeed in any environment.',
        'about.p5':'I have collaborated with many people, learned constantly, and kept reaching. I will keep embracing change and seeking new experiences ahead.',
        'about.cta':'Download Resume'
      }
    };
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        // attr: 점(.)이 들어간 키(common.process)는 jQuery .data()로 조회 시 깨질 수 있음
        var key = $(this).attr('data-i18n');
        if(dict[lang] && key && dict[lang][key]){
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

    // Lazy hero video iframe injection
    var heroDiv = document.querySelector('.hero-video');
    if(heroDiv && heroDiv.dataset.videoSrc){
      var observer = new IntersectionObserver(function(es){
        es.forEach(function(e){
          if(e.isIntersecting){
            var iframe = document.createElement('iframe');
            iframe.setAttribute('src', heroDiv.dataset.videoSrc);
            iframe.setAttribute('frameborder','0');
            iframe.setAttribute('allow','autoplay; encrypted-media');
            heroDiv.appendChild(iframe);
            observer.disconnect();
          }
        });
      }, {threshold:0.1});
      observer.observe(heroDiv);
    }

    // Hover play/pause on videos
    $(document).on('mouseenter', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.play().catch(function(){}); }
    });
    $(document).on('mouseleave', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.pause(); v.currentTime = 0; }
    });

    // OVERVIEW 이미지 마우스 오버 시 호버한 이미지 옆에만 설명 툴팁 표시 (car-classification)
    $(document).on('mouseenter', '.proj-hover-desc-item', function(){
      var desc = $(this).data('desc') || '';
      var tooltip = $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip');
      if(!tooltip.length || !desc) return;
      var r = this.getBoundingClientRect();
      var pad = 16;
      var ttW = 280;
      var ttH = 80;
      var left = r.right + pad;
      var top = r.top + (r.height / 2) - (ttH / 2);
      if(left + ttW > window.innerWidth) left = r.left - pad - ttW;
      if(top < 12) top = 12;
      if(top + ttH > window.innerHeight - 12) top = window.innerHeight - ttH - 12;
      tooltip.css({ left: left + 'px', top: top + 'px' }).text(desc).addClass('is-visible').attr('aria-hidden', 'false');
    });
    $(document).on('mouseleave', '.proj-hover-desc-item', function(){
      $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip').removeClass('is-visible').text('').attr('aria-hidden', 'true');
    });
  });
})(jQuery);
