<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Guest.aspx.cs" Inherits="CodeQuest.Guest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="Learn HTML, CSS and JavaScript with interactive CodeQuest courses." />
    <title>CodeQuest | Learn to code. Build what's next.</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&amp;family=Space+Grotesk:wght@600;700&amp;display=swap" rel="stylesheet" />
    <link href="Content/codequest-home.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>

            <button type="button" class="menu-button" id="menuButton" aria-label="Open navigation" aria-expanded="false" aria-controls="mainNavigation">
                <span></span><span></span><span></span>
            </button>

            <nav class="main-nav" id="mainNavigation" aria-label="Main navigation">
                <a class="active" href="Guest.aspx">Home</a>
                <a href="#courses">Courses</a>
                <a href="#tutorials">Tutorials</a>
                <a href="#about">About</a>
                <a href="Contact.aspx">Contact Us</a>
            </nav>

            <div class="header-actions">
                <a class="login-link" href="Login.aspx">Login</a>
                <a class="header-cta" href="Register.aspx">Get Started</a>
            </div>
        </header>

        <main>
            <section class="hero" aria-labelledby="heroTitle">
                <div class="hero-content">
                    <p class="eyebrow"><span></span> Learn &middot; Practise &middot; Build</p>
                    <h1 id="heroTitle">Learn to code.<br /><em>Build what's next.</em></h1>
                    <p class="hero-copy">
                        Master the foundations of the web through interactive HTML, CSS and
                        JavaScript courses designed for beginners.
                    </p>
                    <div class="hero-actions">
                        <a class="primary-button" href="Login.aspx">Start Learning <span>&rarr;</span></a>
                        <a class="secondary-button" href="#courses">Explore Courses</a>
                    </div>
                    <div class="hero-stats" aria-label="CodeQuest statistics">
                        <div><strong>25K+</strong><span>Students learning</span></div>
                        <div><strong>120+</strong><span>Interactive lessons</span></div>
                        <div><strong>98%</strong><span>Beginner friendly</span></div>
                    </div>
                </div>

                <div class="workspace-preview" aria-label="Interactive lesson preview">
                    <div class="window-bar">
                        <div class="window-dots" aria-hidden="true"><i></i><i></i><i></i></div>
                        <span class="lesson-label">Lesson 04 v</span>
                        <span class="live-label"><i></i> Live</span>
                    </div>
                    <div class="editor-tabs">
                        <button type="button" class="active" data-code-tab="html">index.html</button>
                        <button type="button" data-code-tab="css">styles.css</button>
                        <button type="button" data-code-tab="js">script.js</button>
                    </div>
                    <div class="editor-grid">
                        <pre id="codeContent"><code><span class="num">01</span> <span class="purple">&lt;!DOCTYPE</span> <span class="blue">html</span><span class="purple">&gt;</span>
<span class="num">02</span> <span class="purple">&lt;html</span> <span class="blue">lang</span>=<span class="green">"en"</span><span class="purple">&gt;</span>
<span class="num">03</span>   <span class="purple">&lt;body&gt;</span>
<span class="num">04</span>     <span class="purple">&lt;h1&gt;</span>Hello, Web!<span class="purple">&lt;/h1&gt;</span>
<span class="num">05</span>     <span class="purple">&lt;p&gt;</span>Welcome to your
<span class="num">06</span>        coding journey.<span class="purple">&lt;/p&gt;</span>
<span class="num">07</span>   <span class="purple">&lt;/body&gt;</span>
<span class="num">08</span> <span class="purple">&lt;/html&gt;</span></code></pre>
                        <div class="browser-preview">
                            <div class="browser-bar"><span></span><small>Live Preview</small></div>
                            <div class="preview-body">
                                <span class="preview-tag">&lt;/&gt;</span>
                                <h2>Hello, Web!</h2>
                                <p>Welcome to your coding journey.</p>
                                <button type="button">Get Started</button>
                            </div>
                        </div>
                    </div>
                    <div class="lesson-progress">
                        <span>Course progress</span>
                        <div><i></i></div>
                        <strong>65%</strong>
                        <span>complete</span>
                    </div>
                </div>
            </section>

            <section class="section courses-section" id="courses" aria-labelledby="coursesTitle">
                <div class="section-heading">
                    <div>
                        <p class="section-kicker">Choose your path</p>
                        <h2 id="coursesTitle">Start with the web foundations.</h2>
                    </div>
                    <p>Learn step by step, practise in the browser and build projects that prove your skills.</p>
                </div>

                <div class="course-grid">
                    <article class="course-card html-card">
                        <div class="course-icon">&lt;/&gt;</div>
                        <span class="level">Beginner</span>
                        <p class="course-code">HTML-101</p>
                        <h3>HTML Foundations</h3>
                        <p>Build the backbone of every webpage with semantic structure, forms, media and accessibility.</p>
                        <div class="course-meta"><span>5 modules</span><span>20 chapters</span></div>
                        <a href="Login.aspx">Log in to learn <span>&rarr;</span></a>
                    </article>

                    <article class="course-card css-card">
                        <div class="course-icon">CSS</div>
                        <span class="level">Beginner</span>
                        <p class="course-code">CSS-201</p>
                        <h3>CSS Essentials</h3>
                        <p>Style, lay out and create responsive interfaces with modern CSS fundamentals.</p>
                        <div class="course-meta"><span>4 modules</span><span>18 chapters</span></div>
                        <a href="Login.aspx">Log in to learn <span>&rarr;</span></a>
                    </article>

                    <article class="course-card js-card">
                        <div class="course-icon">JS</div>
                        <span class="level intermediate">Intermediate</span>
                        <p class="course-code">JS-301</p>
                        <h3>JavaScript Basics</h3>
                        <p>Add interactivity and logic to your websites with JavaScript essentials.</p>
                        <div class="course-meta"><span>6 modules</span><span>24 chapters</span></div>
                        <a href="Login.aspx">Log in to learn <span>&rarr;</span></a>
                    </article>
                </div>
            </section>

            <section class="section tutorial-section" id="tutorials" aria-labelledby="tutorialTitle">
                <div class="tutorial-visual">
                    <div class="terminal-title"><span>guest_tutorial.html</span><i>Free access</i></div>
                    <div class="tutorial-code">
                        <span class="line">01</span><span class="purple">&lt;section&gt;</span>
                        <span class="line">02</span>&nbsp;&nbsp;<span class="purple">&lt;h2&gt;</span>Learn by doing<span class="purple">&lt;/h2&gt;</span>
                        <span class="line">03</span>&nbsp;&nbsp;<span class="purple">&lt;p&gt;</span>Try every example.<span class="purple">&lt;/p&gt;</span>
                        <span class="line">04</span><span class="purple">&lt;/section&gt;</span>
                    </div>
                </div>
                <div class="tutorial-content">
                    <p class="section-kicker">No account required</p>
                    <h2 id="tutorialTitle">Learn the basics as a guest.</h2>
                    <p>Open beginner tutorials, read simple explanations and test HTML, CSS and JavaScript examples before registering.</p>
                    <a class="secondary-button" href="Tutorials.aspx">Explore free tutorials &rarr;</a>
                </div>
            </section>

            <section class="section about-section" id="about" aria-labelledby="aboutTitle">
                <div>
                    <p class="section-kicker">Why CodeQuest</p>
                    <h2 id="aboutTitle">Build real projects.<br />Apply your skills.</h2>
                </div>
                <div class="feature-list">
                    <article><span>01</span><div><h3>Interactive learning</h3><p>Write code alongside every lesson and see the result immediately.</p></div></article>
                    <article><span>02</span><div><h3>Progress that motivates</h3><p>Track chapters, quiz results, learning time and achievements.</p></div></article>
                    <article><span>03</span><div><h3>Help when you need it</h3><p>Premium learners can ask the course-aware AI assistant for explanations and hints.</p></div></article>
                </div>
            </section>

            <section class="final-cta" aria-labelledby="ctaTitle">
                <p>Ready to begin?</p>
                <h2 id="ctaTitle">Your next skill starts here.</h2>
                <div>
                    <a class="primary-button" href="Register.aspx">Create free account &rarr;</a>
                    <a class="text-link" href="Login.aspx">Already registered? Log in</a>
                </div>
            </section>
        </main>

        <footer class="site-footer">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home"><img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" /></a>
            <p>Learn &middot; Practise &middot; Build</p>
            <p>&copy; 2026 CodeQuest</p>
        </footer>
    </form>

    <script>
        (function () {
            var menuButton = document.getElementById('menuButton');
            var navigation = document.getElementById('mainNavigation');

            menuButton.addEventListener('click', function () {
                var isOpen = navigation.classList.toggle('open');
                menuButton.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            });

            navigation.addEventListener('click', function () {
                navigation.classList.remove('open');
                menuButton.setAttribute('aria-expanded', 'false');
            });

            var snippets = {
                html: '<span class="num">01</span> <span class="purple">&lt;!DOCTYPE</span> <span class="blue">html</span><span class="purple">&gt;</span>\n<span class="num">02</span> <span class="purple">&lt;html</span> <span class="blue">lang</span>=<span class="green">"en"</span><span class="purple">&gt;</span>\n<span class="num">03</span>   <span class="purple">&lt;body&gt;</span>\n<span class="num">04</span>     Hello, Web!\n<span class="num">05</span>   <span class="purple">&lt;/body&gt;</span>\n<span class="num">06</span> <span class="purple">&lt;/html&gt;</span>',
                css: '<span class="num">01</span> <span class="blue">body</span> {\n<span class="num">02</span>   <span class="purple">display</span>: grid;\n<span class="num">03</span>   <span class="purple">place-items</span>: center;\n<span class="num">04</span>   <span class="purple">color</span>: <span class="green">#07101d</span>;\n<span class="num">05</span> }',
                js: '<span class="num">01</span> <span class="blue">const</span> journey = <span class="green">"CodeQuest"</span>;\n<span class="num">02</span>\n<span class="num">03</span> learner.<span class="purple">start</span>(journey);'
            };

            Array.prototype.forEach.call(document.querySelectorAll('[data-code-tab]'), function (tab) {
                tab.addEventListener('click', function () {
                    Array.prototype.forEach.call(document.querySelectorAll('[data-code-tab]'), function (item) {
                        item.classList.remove('active');
                    });
                    tab.classList.add('active');
                    document.getElementById('codeContent').innerHTML = '<code>' + snippets[tab.getAttribute('data-code-tab')] + '</code>';
                });
            });
        }());
    </script>
</body>
</html>
