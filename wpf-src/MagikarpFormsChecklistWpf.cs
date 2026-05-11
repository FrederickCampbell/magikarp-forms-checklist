using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace MagikarpFormsChecklistWpf
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;

            using (var mutex = new Mutex(true, "MagikarpFormsChecklistWpfSingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("Magikarp Forms Checklist is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var app = new Application();
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                app.Run(new MainWindow());
            }
        }
    }

    internal sealed class FormSprite
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string Base64 { get; private set; }

        public FormSprite(string name, string slug, string base64)
        {
            Name = name;
            Slug = slug;
            Base64 = base64;
        }
    }

    internal sealed class MainWindow : Window
    {
        private readonly Dictionary<string, CheckBox> checkboxes = new Dictionary<string, CheckBox>();
        private readonly TextBlock counterText = new TextBlock();
        private readonly Image previewImage = new Image();
        private readonly TextBlock previewName = new TextBlock();
        private readonly TextBlock previewHint = new TextBlock();

        private readonly string saveDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Magikarp Forms Checklist"
        );

        private string SavePath
        {
            get { return Path.Combine(saveDir, "state.txt"); }
        }

        private static readonly FormSprite[] Sprites = new FormSprite[]
        {
            new FormSprite("Normal", "magikarp", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADkElEQVR4nO1Zu3HjMBAFNa6Aka8MpVeCQ4WuQeWoBoUKXYJTl3GO1AJulsJSD8u3AGhSn5u5ndFIJMHFe/sFoBD+S10Or28xhCCff1ai+X5qicT69NnTCYD1QK9OYBNuT6i75RybWyrff3/YW0LmqUMqmt8MYCyE28MlOsnLxjw3gcqY5wmjw7VJtQKx42J4oIyTn3fbVhKrWn+zxOKH17fhOoEXElVQ8s7D1zPnK+ABvJIALyjBmoXjXYGjWBJIwF57RA51gstCSCbYf390tpsi+NoEEjLvv/8EQkR1L5KSgmEZICD701fXClikP31pPgxy/PylJIZn0JG7W3kgAw/hMIAREAlIkwh4ea810efIC7knYIdva/UL6Is30rWOJZXmYnFH7JpoHQ+kakIHgutROuYJtbha3YTQ4NG1SGwqq8cRQCFeOwVqdSkRBS+CYRlWkJGAVghmUY9Yy3OM/6Qfcyss9cKGlEsaFkaayqeTxKMH+lTdFhNgGw8kQdp/V1seWJ0eibAwFzAH3MWYNiJHR8QYb13zzGmGZNUbmSVRkd7PqhKpRG7VwuSV37U8KnhXwjtrjIilswONkglABOOBhwkyfS3W7hMwBhyLQcJwJaCA7aSiiIVICwgEzEqtFQVlDafvSqFJ+aPjpp1YYte6O1l9otiKekfLo7GYV3Ei/B7m0E6u74sBTeUa33mZudmQ6uSG1XkXwnvYZhYLDXLebdVrCfzVaKTsZovAYhJrc9MSyLxgk5UBl8m9bn1OuoQAGrAFPAuhLKkVfMkLptJ0DHR/kvuT5pgZogZe9dnwRgJjbMEmZLKWYYLgdSIHNAV/JLmiXVqenXchJe60c09usKpDJmD9YZJDmnwl8Ew/eh/epxsglmRDIjGr16oR1P4sFC0QlkNHU3EYLoaX7sgEZK1us+cAbGz3oksA6VLAEt9DTOvYsHBHdlX8eekJrLzaZbK+Y0JrtJp6Dsf3M7als4ScHNvjkezoxDtSIad19uxIBA/G3KWGdwTjesCIWjG2nkSo4KYFDgNo8ehPl/KZxnaLjmDY+b25l32YR7wlsJEIY5E48+BqBLL7loyOM6FRAhIL4bMegcJ9e0rtfc/ywo8JqCKmkHiheG6K3z/xwmHu+WnL/7okF2pS1RmIF5aALwJryAUPPLsukZgdPs1ZXwmluaRQvB5yl/8OWAPEZ62WjXcjAYBardwKqAn8Xzx8Z3tbhDM8AAAAAElFTkSuQmCC"),
            new FormSprite("Skelly", "magikarpskelly", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADgElEQVR4nO1ay3HjMAylNKnAp2wZvm4JOfqYGlKOasjRx5Tga8rYnNQCd8AhFBB8IKFI/uzMYsZjWaLA90D8yCSE/9KX6fklhhDo889KVN8PLRFYHz57OBFgLdC7ExjD9QkN15xjvKbyt68PfYvIPLRLRXWNAMaGu91dohG8aMxjE+iMeRw3mr6LlBeIHhfDHWWZfD4dvSR2tf64xeLT80v6ncETiS4oeufu/cz8DTiBZxJiFZhgz8LxpsClaBKSgP5tEZn6BLe5EE3w9vUx6GoqwfcmIJd5/f0nACKse5O0FKQ2gEAezp+DFzDJ4fzJ8ZDk/fIrkch6uL3Ypc0YPeCFOyQwBJA+XiHw9J430NcIsgCBhYMz6KE3li2O7rFBgK7tK5CzCRxoTDiglWCLs9URIdDYbScAusckGYhlrYGBal1MBIDfTUa+4AyBLGoR8zyX/q9l3iEeRpAuoVsocaVPI4iXxHDI2W0zAbTxkCRA+R967YHWaa3EvHEVZAyYzRgXIkNHRD6+d88zlV3vgqVIieKa7xdZyZtGSWTw0nUvjhqrS+5d3edaNYCBcnAFUIKxwJOoOHJX8kM2EgIuWxF+hlgVk9JLa9Mgg5CArUwkRQCOCDglGl0En5Dv6uXOVk+KW1bn1eHAlKAbjVsU12kOxsDAVQUvCuDTysCj7GS61XwK4TUcF2t7u835dORVS+Blagdpt1gdSaCKAWmBcFlqRQWenreA0+RWtQ6ipZAG9IDXBBbgFfjGKqhMMyDQhzPdr4pj0sHB3gPP+rR7SwKFJSSwXhBL8DyRAXoBL+d7B7HCVZqezaeQA7eu3Ho/QOmrACwrqLUFzFZJOy451kibzUTA7/L7RKjVcsD9gCbhzUYi9xeuKHZj1WZpysGqMw7ChfDCHRkp7OXtRl/Dky25m4Ei4m/Cp3vWRlLVgULxZcnH1XPUnIkakISCna3GK3eTY5WpPjnWxyPF0Yl1pAJO6/TZETrdg2LFH1wusT/QfrdkD+0K+iQCCYqRkHXKwpXHonn9gs7v1b3ig1bEaoE7fy9IYqzgbgSK+5oMj1MHYC0gEehH82wj0Lhf+bHxHW9CYMqKkEKwCs1zU/n9k1WY1p6fev6uC2KhJ12dARDYAr4JzBELFbDGb0jip/+e4H6p40prSa1drX0EWAkVQPnMEwvhpiTEJF4rewG5vOEvYt9Ccx/9NeUAAAAASUVORK5CYII="),
            new FormSprite("Calico1", "magikarpcalico1", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADlklEQVR4nO1ay23jMBClhFSgU7YMX1NCjj66BpejGvboY0rw1WUkJ7XAxcgc5nE0Q1I2hSjADiBI1mf43vwl2Ln/Upbx9d0752j7teLFftfiFeur13YnANYC3ZxA77Yn1G25Rr+l8vPXhzxFZHYdUl4cawB9Jtx+XLyRvNo9+yZQuGc/YTR+N6laIPI+735Q4uLT8VBLoqn1+2csPr6+z78DeCJRBEXP7G6eGQMJ8AITLFnYb+4BBn7++ui0pkSgT2+f8ty8hfMmkbGBN15K4APwBejaBQhk0DGHHOhTda+VnILFGFALfLjcOB9m+Xv9kwCHjty5jUIoUQ7xPYMhgLTVCoUShJEcJ5qHEIGd9/LCHfQtEhsuN75Xie00L4Q0I9GJhRcJyRIsrrlcJUGeIiF9dMx6h8u3AVyDMOoL0yOCsRbqGKzUJcGvLQCrCHCMarFtEau5ziRQgET3bCj1olzOW0WCZhelMMEar5FoJb314oEklIYjcycec4xLnZJEKy/0cGyOCtxRDR1q4reeecZ06vWSgKwMCy8YpVWtQFK4d5BXuIcE3V682PiKUaZDLBF4yAP+2VkAQ1dFUlGUEpnoq6lAQyjXjIfWYL04UwUMnSyNi0VzvUFZPBkXEHBNEp8DKGk4fpa8QDqxJy06cRi4knMwy2RDBq1G9yLozODm4TgOfdTJsRmScQL4JOlfViYe5YUZVtPRuZM7JBZzFTLdw2QmwxjYaAxagI96szHLMcclUPMCdloLOC1udesp6CICaMAa8FoIJUnN4HNeQPAIHEEPFzq/aI6JIUrgWZ8MbyQQY4uUIbBSEiN4XsgAvQA/3Ls2VpcOCwLpmo4uJG5S5SLoRLSqoyRjYj3uFzKHOPly4N2yPEZC4nl1clXHY/TAmmoEjS8JRQlE1vYTjBkGaZOA+kZGIEt1W7sOpGJXJV0QJgl4Eoxpvte1eKmfFV/vPUErr7MnrunXBqwqJJTsbDX2XKZbtxPly7H8zpN8kcNvQvj+LO815h4PH8ZMQhXfmLIENABRELymT/noleicjodoHMUAGo7HSIhz6niLoK0ROEfAIO1aE0jOSzJoTdjngHiNhLLOcwQy55OcyOxNLzg7zh8PH02h4gU1fLR9yQtOWXNV8hoAc/fUxmhRp1MIPAM+C6wiFxbAMr9VEo/+PaH6oUIorSWFUpv4z4tiJa0B4rWaXHCiIW5LAgDVWrkWUBX4fxlhL8XfxWYJAAAAAElFTkSuQmCC"),
            new FormSprite("Calico2", "magikarpcalico2", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADmUlEQVR4nO1ay23jMBCljFSgU7YMX1NCjj6mBpejGvboY0rI1WVsTmqBi5E59OPo8SNbQrTADiBIpqThe8P5iYlz/6Uuw+u7d87J8c+KN+ddiyfWp/d2JwA2B3p1Age3PaFuyzkOWyo/f3/aISGza5fy5poB9AV3+3HxmeBlz+ybQOWZ/bjRcC9SrUDsc979oMTJx9OxlcSq1j88Y/Hh9X36HcALiSooeWd3/cwQSMAqKMGahf3mK6DAz9+fHStKAvrj7Y8dm44wniUyrLAaLzXwAfgMdOsEAjLomFwO9FHdS6WkYNYGtALvL1eNh0l+f/1KgENF7txGLpQoB/+ewAhAOVpFXAncyLYTq7uQgJ3O9sYN9DUS6y9XfZb4dhoXRlYj0ZmJZwGpEizOlpySkJUSEX1yrXr7y90AbgU3OlS6RwSTm6hTsFaXBW/iaJUViATUR5lv54i13FcShWTg3bMEIF1OR0OAFicVN8Ecz0isJYfchweSIAXHxk68Vh+3Oi0JEzf+KQKghCrSiprRQQN/7Z5nSLtebwnYzDBbhUxqpRnIitYOWRWtIcRNPTMSaWU6xBKBhzjQn10OYKiqSCoKSZGJvpZK3od0rXhkDtWLPVXA0NnUOJu0VBvI5Em7gIBbgvgcQFnD6buyCqITa9KsEoeGKxmDXqboMmg1eRZBFxo3D9ex6ZNKjsVQjBPAJ1X8ZWHgSVxk3Wo8OffhjonFXIOMNzeZyCgGNZqCNuCj3qLPqs9pCmSrgJU2B1wmz1XrMegSAmjAFvDMhZKgVvClVUDwCBxB9xcZn2edEQxRA6/6rHsjgehbogyB1YIYwetEGdAz8D2xfBifzqJrPLkQuEmWi6ATYVmHBGNiPc3pNoY0+ErgRSwBXH2b2ZwR2h7jCizJRlCcEle0QCBonc5lMw7DxfDSLzIBWcvb7D6QilVVdIXmzlvwIujT+qxb46N+Uvx1qwksvep9O2ZcK1pNV27Jp+jDQnaO7T5PbKr0e1n3hPD72ehI3kPd431jLNtqNOwxFQlYMAkQBM/0kU2vROd4OkbjEAMwHI+RYCuQ6yIBVG3z1yOBDGm3NoFk3JJBa8K5BMQzEmSe5wgUxuO19edGEr6wBfm4+zCFZBWo+7BzbRUcIbEoeDMAS8+0+mjLLoS3BJ4BXwTWEAs58Ow3JfHovyc0v1RxpaWkUJLU7LYUYiVWABNgZNwRsX8w2VyWWLkVUBP4v4IjM7nnBN/cAAAAAElFTkSuQmCC"),
            new FormSprite("Calico3", "magikarpcalico3", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADc0lEQVR4nO1ZwZHrIAzFmV+BT9kyct0SctxjanA51LDHHLeEvaaMv6e0wB85CAshgRw7if/MaiYTB4N4T0hIEOd+pS1+fwzOOfj8txLY96YlCNYX321OCFgN9OoEdu7xhLpHzrF7pPLh54s3AZlNu1RgzxLAUHG3l0tQglfqs20CjT7bcSM/JSkrEN4vuBfKPWBWtf5uicX9/pj5+vXj0AQFY15ez1w/DukbPkK2RYItCwf3qkKMk6CgsC22q0R8m+AyF4IJhp+vjmdTCr41AbjM6f0vPBYrNdx0L5KagrEMAJD9+dK1ANM+/fmC8TDK5/fbSCL2wfJilTLjjwU8BQ5gsERAkAheEwAP45y7SOXE6i4Eviu6CViWuFQHv+fsNETXagVexyYO0V8LiWCliQtXAKA3i0/WR71stcJSMrtG9ThKBKNN1Em7Ceji4K2BfxcBBCG5hUZMe0+tPPl/KVdD4jMRYNul6ttETNunRMKyo80mIB08KAkhKHnsFIq5Tm0lrgtXgcaAWlFiIlJ0FAkplhfugVVvEIFItTovE4QxhWB9xGol68d6pzS2JcvFOMCfnbbNKf2SIGEWR2a/7+N2jfOAi2Hc0N0uYug4ADpBGmStWRj4IoO3ZIigMJnysYADSxXEV5QS4Ls8AAkJNfFg8kKrQV8KumKEQJ7HOW7xMwU91lGkLktjMgKGwBNJAMH+DMCdO7lDZrGWQpDoJvCYgj8aI6vHWDE4Ck/rWTv6HG6B0irQ8kMDTksLKsO0WiMBakALeMmFMmAIXuiTFMFEJ5fcp5NA92doL5Jj5uct8NRFa5KdoOh2WLukZedi01GSntr8/pg+vCPOr23ZhY/yXYcvvxLMeNuW6SKHmAw831Y/mX66+mS8uXK1nGFrt27F6tSSoY86cQUqucJcbiSFNQKNq0ORiPGuVJMw50g5BgtmQ6VYK84BJBHRSVOwP+1Wzpeuwq9HVOuzKxXeN9Pn2PtaqTHrCqYCtEZAAl/00XR6svMJ4+evlESCtTXLW0sJ7JQYoBdjs8EbCGTtnAy1JvmuAQkSCWGeZQQq7YUfK9/hKQQ82Zu1d0R50Ucjcc8q+Ln3p5b/dYVYaElTpxMILAFfBWaIhQJY5TeVLCvf4z7mQQ1Xmktq7mqtI4KVpARI31liwT2VBJnEamUrIJM3/AOWFif9UqRtqwAAAABJRU5ErkJggg=="),
            new FormSprite("Twotone", "magikarptwotone", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADe0lEQVR4nO1Zu3HjMBAFNa5Aka4Mpy5BoULX4HJYw4UOXYJTl3GO1AJuFsJKD8sHYGlSn5u5nfGQAoHFe/vDxyH8l76Mu30MIcjfPyvRPB9aIrE+/fZwAmBroFcnsAnXJzRcc47NNZW/fX/YJiHz0CEVzTsDGBvhdneJleRlfR6bQKfP44TReFmkvEBsvxjuKOfJj4dnL4lVrb9ZYvFxt0+/M3gh0QUlY+6+nzleACfwSgK8oAR7Fo43BY5iSSAB+7tGZOwTXBZCMsHb98dgV1ME35tAQub15U8gRFT3ImkpSNsAAbl9/xq8gEW271+aD0l+f/5SEukbrMjDtTxQgIdwSGAERAbiEgEv47yJPkeeSJuATU9r9RPokzfyb+1LKs3J4hWxe6J1PJCrCe0IrkcZmCfU4mp1E0LJo2uR2HR2j2cAjXgdFKjVpUQUvAiGZVhBzgS0QjCL1oh5vmP8Z/2YW2GpFzakXNKwMOIqn5UkPntgm6vbYgLs4IEkyPI/9LYHVmeNRFiYC5gD1c2YLkQVHRFj3LvnmbMYkl1vZJZERdpeVCVSiapVC5NX3nt51PCuhHexMCKWwXY0SiYAEUwNPExQ6PNYe5uBMeBYDDKGCwEFbCcVRSxEPCAQMCu1VhSUNZyOlUKT80f7TVdiiV3r7mz1iWIr6h0tj8ZitYoT4T3NoSu5jhcDmsp1HvM087Ah1akaVsdDCK/hubBYcMjx8Kxey+AvRiNlt9gENpNYFzctgcwLNlkZcJm8tlofsy4hgAb0gGchVCS1gm95wVSagYHevkv7ZHFMOnQxw7MCA6/6etVscoLS46I9jOCpC7+RvnQeTPBxt09j2Dg4c1OdkxiFbUXRpu8sjLRk2hzS5CPgi/bR6EfvQz/3Achzhi08QA70xRhT/yfWD+CFxlrh3m7Yu0tKAG8iKiTY/wk8d6U1id4TWRJJFl0NK5u1IXyWk9qVU5Jd3a6h5wG1WMjNsU3uOW5mN3HojagfW1uNWVcwDaAtAt5rRapzzHPiBcJM3X0Spq0a7wqktgVuEVAxORXWJlC0WzJoTXi2gESin82zjECjfRLHlWe8CYERanPtGyin4cOeP/HCOPf+1PN/XZILPenqDITAEvBNYI5cmABr/EYpVuWfhI97UCeU5pKa6611hFiJLYD4zZML4aYkYBKvlb2AXNHwF/gVTcmbiB6IAAAAAElFTkSuQmCC"),
            new FormSprite("Orca", "magikarporca", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADf0lEQVR4nO1Zy3EiMRCVKEfAyRsG1w3BR47EQDjEsEeODmGvDsM+TQraaqEWb3q6pR6YMbhqu8oFM+jzXv8lh/Bf+nJ6fUshBPr7sZLE51NLUrSv/vZ0AmAt0IsT2IT1CcU199isufjx612+IjJP7VJJfNcApoa7PVySEbzamOcm0BnzPG50uhYpLxA5LoUHSt182O+8JBbV/uYejZ9e3/JzAU8kuqBozsP7meEKOINnEmAFJtjTcPpW4CiSBBKQzxaRU5/gfS5EGxy/3qOspgi+twG5zOH3Z1CI8Np3SWuB3AYQyO35I3oBk2zPHxwPWf78/cUk8m9QkeNaFhiBB3fIYAhEAeISAk/zvIE+R16UdwQ2f0qtX0BfrFGeeaySaS4aN0T2RMtYoGQTdSCYHiVqlmCNs9aFC2WLLkUiimeVAAFoBRwRR4C8Fq3PiUBYkt0zLNZuY4HSUmNneo0RfCfWnmSvwV+9XeAngBoE1PGtNUWxw9hKdxOw+nastK3J2E601lyDxEZsqC7EhchYo/o/kpA9jyQ4pxgqXW+dgwGEC0UtqJWgM7MWZp6SBIJTIj6UJDAqjIglyoEtAhKMBR42GK3n0fa2ANOAc1omKRiuBBiw3BRTpFcYBALGzS05FlBScTyX0zF6wqQSk+9Kc0MdMF0GxtV2QWjMyvcJvuc9uJLzfFIg1I5RAXyZedigymu61bAP4RB2I40Fhwz7HVutgL8qDfsxAB9dQczpk1sDzQoyWDXgtLnmQkfwcSKACvSA11xoFNQMvmUFkWmiBnp7pveTnqmuQeCw1mjgeT3p3kig+hYcQkYatgTB80YGaBf4AD0VrTXsQ5KdMIK2TmE17SnBqNWHSQxx8FngLQJofdkcBiFakOVAkiQ82Qhy/8gVJRCp/QMceAzSJoHguLusgveZ2Ncojd+kLeFWAHuhIq5+y2pltBNZFgoWroZaekWt4RzhHlVrbDmh3RTWEOXmWF6PuK5UlFZ8tF4o3z2N3awrmMbVd6v9lcA1Uc8WA7iQMX++pTQS4p3p7wzEaoFbBFhmnAZvIjB6L8kYR8gWkNSx2jIEGu/rd+Pc2yORGpnodvfppNLQch/t8xYrnOben3r+r6vEQk+6awbFCveAbwJzxIIFXntukZjtPu6o77jSXFIoc+6jFv/fgVYA8TevZtO3kQBAXi17AbnA/wO6c6JMUKTzfwAAAABJRU5ErkJggg=="),
            new FormSprite("Dapples", "magikarpdapples", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADeklEQVR4nO1ay3HbMBAFNa5AJ6UMX1OCjj66BpfDGnLU0SXk6jLiE1tAZikstXh8AJYi9clMdiYjEiQW7+0XoBPCf2lLfzjGEIL8+2clwu9TSyTWp8+eTgzYEujNCezC7Ql1t1xjd0vlH9+fOCRknjqkIlwzgLESbg+XWEhe9s5zE2i88zxh1F+alBcIvhfDA2VafHh79ZLY1Pq7NRbvD8fxPoEXEk1QMufh+5nhAngErySMF5Rgy8LxrsCtIAlLAO9LRPo2wXUhJAt8fH922E0t+NYCEjLvP/8EQkR1r5KagnEbICD3p6/OC1hkf/rSfBjl1+8fSmJ8ZjpydysPZOBNOIxgBEQC4hIBL/O8ib5EXsiYgB1/0epn0GdvpHt9l1Sas8ULgnuibTyQqgl90bjeSsc8oRZXq0MIjR7disSusXucAFTitVOgqEuJKHgRG5ZhA5kIaIVgFi0R8zy38Z/029wKa72wI+WShgWIq3wWknjywD5Vt9UE2MHDkiDtv2ttD1BniURYmQs2B4qbMW1EBR3Rxrh3z7OkGZJdb2SWtIp0PKtKpBIVq5ZNXrlu5VHFuxLeWWO0WDp8EZTMAFowJfBmgUyfx9r7BIwBt8UgYbgQUMC4qChiIeIBYQGzUouioNBwOlcKTcoffW/eiSV20d3J6jPFKOodLY9gsVLFieZ6XEM7uc4XA0Llmua8LDxsSHUqhtXwFsJ7eM0s1lKYREEl8BejkbKbbQKrSazNTUsg8wImKwMui5e69ZB0SVhYA3rAsxDKklrB17wAlaZjoPcnGZ81x1GHNjN7VmDgVV+rmk0nKHtcxAXw1GWf4X1pHZvg/eE4zmHzzJmb6pzFqNlWZGN6rWFkq4yWTMwhTb7WQakH/db7Zr77ADSzBLF05gFyoM/IQ/2fWX9Iusm7SNwlLGSmxZAkHu5Zuzdz6bfSwRl2bJC5ZNpgaU/A6qDv4KJQVVjVYPe28zbP6MEj5Msxfh7Jkhasn40XvsTZ8WjnODBdRYABQPF+VqQ6exOShMjy7TYjAWPFeFcgpS1wjUDDg5sRyMaRDEvwBpBI9LN11hGojM/iuPAb70Kg590XCYRa+LDfa7zQL/1+6vm7LsmFljR1BkJgDfgqMEcuzIBV7imJa/97gntSI5SWklrqrW2EWIk1QPvMkwvhriTMIl4rewG5ouEvd91kGr3cHpIAAAAASUVORK5CYII="),
            new FormSprite("Tiger", "magikarptiger", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADmElEQVR4nO1Zy3HbMBAFNa6AJ6WMXFOCjz66BpXDGnLU0SXk6jLiE1tAZiks9PC4+FAkI2YmO+ORDILY9xb7A+Tcf6nLcH71zjn5+2fF0+ehxRvWN58dTgBsDvTmBE5uf0LdnjpOey5++frgISFzaJfy9N0C6Avu9nTxmeC15hybQGXOcdxouBepViA8z7snSlQ+vn1vJbGp9U9rLD6cX6f/A3ghUQUl7zy9nxkDYCAykYBdiOOVZf1fBY7CJJBAK5GhTnCdC4mCy9dHx9UUXEbAmu/quLjM+4/fziCia+9GYFIAllbAptX662cErt9FBPzPX9+m8VCZN3WbUw68WF3A9tfPDoELGAGIIHO7wCRo/U3E2sKsWwTQXW2ugA1uk4zJDtAOdSuwzxcQH2XFBfBVEmh9aOy4oes2cyGje0QwOUUduUdcS8GrUYQo1YzVEglohkDfrhFreY5BjOtLbLkN4uFkpMvOIkFSVYoV1wjiJLutEbRCl/Pt4MNZX7ViR4zAboLuRIHsH40FjIFsM6aFKLOGGfgtPc8IMbGw6/VMIEmNOmbk+lkr3BKMWjskVrCG9Gk9MQ2owNFIiCUCD3Ewcy3LDXReCTzF0bRei7X7kK4Jz6QL4yg862Z+z0pLtaEGAgFbqZZFQaHhZC11R21tsCa98CIymdMiBHHRZbDaylyyWC5IPXyfdNwA3zLXcL4FvrY0If3Gd14WBp7ERdatxjfn3t19q1u7zfHeXgTwd6NhPwbg47pW8MZxLW7QDsx2AVNjDrgoz1Xr0XAVXaMG3nKhJKgVfGkXEDwCR9D9VcZnxTHrjhZ4Xa/WFcyOiJrG+DCiz2FOBN5ylMT3XVjfeg/O3L7lPCDpK6mWwli3lhVoqtSDijxXq+Nh3wBfZDcEMvK+7Ab0TU0SrV6wdHL+5TMwgyEiM+uPYe0C6em95hNZS/bgoAxtASqbFKoF1Sgl618esLZ5QMF8jC6kSuCClgHM2ubSCa6Hah10lcC3N3zGzTEH8sy9rCsVI/jY3USs+6Z1VzCFq+8kBhg0g8+QsRpCrzozNx/Lzw4WCd4B/ONAxhRcatUdEVAhA7itCSTjTAatCZ8lIL5EcDMChfGkkBU+SyB9xs8fd59cZWTFPCdH4pFdGJben7b8rmvEQk2qazojFtaALwJriIUceOt/FCtdL5LmqK+40lJSKC2Za7ffDqwCyMB43BnCdWR3WWLlVkBN4P8AG0WTt9ACMGcAAAAASUVORK5CYII="),
            new FormSprite("Zebra", "magikarpzebra", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADvElEQVR4nO1ZzW3zMAyVg06QU78xeu0IPfaYGTKOZ8gxx47Qa8ZoT15BH5iIyhP9aMmw06RACRRxJIV6j+KPzIbwJ3Xpn99iCEH+fq1E8/nQEon16dzDCYD1QK9OYBNuT6i75R6bWyrff3/YISHz0C4VzTMDGCfc7e4SneBlax6bQGXN47hRfy1SrUDsuhjuKHnz4f2llcR9rd+DxeUZv7eQuFth6+E+k4CeP+VP54BAJlhRG38UOAqSsCdgPz0ifZ3gskImG+y/PzpbTQ14HRv9fns8iY6we/0KhIjqXiRTCs7XAAG5PZ46C5iBVRL6bD91Dipyd6sTKMCrr8vE4fPfGUQCUhVdp79LRFbz/Q0DL5swNxEA4FIdkkAXErfBMeZea0mHX8RHk7+OBI6eErYiFhcRfUrUuNUqbrSp3B4RjLdRp2CtLgEvc5YggF8smYBmCObbHrGWeTwBEHXPsJTIhqTLwrcdqW6qcaAuw+Jim7LbYgLsxQNJ6OY4z8CioE7Rpe6EY8MlQSxyJ4wB9x6jhcjRQQPfWhwFx4aJ2mL0FdcUSwAtqpMsTY6uwi0pUsikFFzUkG1ZT6gBFTgaCbFk4CkO9GvnARQ30HVT4E0cVSu5TdeIR/dBF0xznQ0i3KCr1YYaCATMUq0VBaWGUyOoO0qiSbcDXReerBJZbIM6WT0r9kRPR4/YWMzLOBGez3tcAH9l4CkV470s/+aplk2MSFy4bjW8h7ALL9narbfN4WrtBP5qNLyPserNgjePa3HTFMhOQcbVxTzgsrlXrQe4paIBW8AzFyqCWsFPnQKCR+AIenuU8XE1HsgF0AOv+qx7I4HsW/ASUljYEwSvGzmgM3is0gcSK1qlL3epkAJ3XLlHAyzrkA0K69lsoaLBZ8HbF5+D0Y+nD7+nN1d6PcYTsACnshHk/sIVLRCbIndwzXBIuwToG5mAtEHHMo8BjWtyVRVdAgg6F8X6Pfi0rg0zZFQHQFkIn9eaYAldrXbC7FMQlWBXq+nJKWk8hdWFdI5te6ToAcE78+jZduJM70gkv29PXTW8Fox7AkbUiqw/dP4knQecz5b3Xk3757comUtJGFea34Jh/XszVvyxU/CuwEYia36hvlnAGwkU45aMriPdO09iheA6BCbGsUPHwNdIxIkW5HwCqogpJKdQ65ticE6ewkCCeHb/tKX9TWKhJlWdIRHA/ZeAnwTWEAseePYdZZSuw0xpjvqKK80lheLVkB/53wErgAUwMh6IFGl5Rcj+hjOs3AqoCfx/CTB0eJxuZfoAAAAASUVORK5CYII="),
            new FormSprite("Stripe", "magikarpstripe", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADqUlEQVR4nO1Zy3HbMBAFPa5AJ6eMXFOCjzm6BpWjGnTU0SX46jLiE1tAZiks9Lh4C4AUZSkz2RkNPyAX7+0PSyiE/9KWw8trDCHI75+VaI4PLZFYn449nABYD/TmBJ7C7QkNt5zj6ZbK91/v9paQeeiQiuacAYyVcLu7RCd52TOPTaDxzOOE0eGySPUCsc/FcEfJk4+/f/aS2NT6T9dY/PDyOl0n8EKiCUreuXs/MybAQGQiAV7I9xtq47cCR7EkkICe67VH5NAmuAn4QixAJMEIOB7ZBHwtB+L+630AS2fghowlF3anz0KZ3H/79Wcz4C0CUw8jYHenzwGBHz9+TAARpCamjFmxYz2JvkRYoyVg6cMJ9MCeVUJyLWCTtYsxERlPfdIQtvRAqia94KdrFi4CXkAyj4hHBXzyTNyUAOkeJ0lAPGsNDKjoUiKoR0JRwHtzrRZcoLCKdK6wxbt4TtqNvHaEK70weUCUScVJVqZhQQCH3hUX8wE9cCSeW0WAfXiwKoPjHliVWogIIQij4RovYA647mzU74gW7u15lOBo1hVPTBhGS2BWGpkXnPrtVi0UDBW7huwu59SAClwIy1z602cz8JQHejl4AKGGF3VdrK7eMHk09Fp7l8q1waMY87mGny2NOME0JopYiJBJMyEFgYB7EnYPOYEtiQLX1gbXpGerhNXoZOGs2BP1hLoYQaf3mUQ4n+Y4Az4nuogYUFsaOeI7zws/NiQvZiTEGkpaJnoL51aiAXom48XaCfzFaNiPAfiBEShyQNvfKYQ+8loxEwWvPRADLpN7q/UIxjAx3gRvCWTgBfiKF5Q0hFkBeneS+8XimEOlB7zqs+GNBHJsYTWxnSWT5IVcac4tNwWdwaNxjiRXksVT/xRS4p7vzaxJwBRVh0xQtNFKBEWTj4FPZAPTj96H9+k+K/0eQA9YIkjAKZ8zvWoQAyS/d0weVhIOaZcA/SITkK26zcYhIfOqKroEkK6o+gzU9oDzOuAXyazl1aP+DEA7xj7mre7ZBkBI55tuwZCdY7urQL8dGAGyE1F8G4yXjTEX5KItmMrWt0vAsb63xTLTOYIH2M7HqnabkWAeqAH1WuAaAZUFX4OrCMzuWzJoTTjWgMQawc0IVO5jaWTgWyQiyZf1BFQRU+gk5Uw8Emu8cFi6f9qzf0lyoSVNnYF44RrwVWAdueCBZ9e1hF4cPt1Z3wilpaRQluxHbf7fAVsAcazXsvHbSACgXiv3AuoC/xdzyqmuLoDBWwAAAABJRU5ErkJggg=="),
            new FormSprite("Bubbles", "magikarpbubbles", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADo0lEQVR4nO1ay3HjMAylNanAp2wZe90ScswxNbgc1+CjjynBV5exOakF7oAm5EcQICFL2nhnFjMeRxQ/7+FHkE4I/6Uvx9e3GEKgzz8rUXw/tURF++q7pxMAa4FencAQtie023KNYcvJD1+fsonIPLVLRfG3BjA23O3bJRrBq/V5bgKdPqu50cuSwQoAb8BuGtgPgR/ff3p321WDeFhQFsTj61t6zuCJRBcUjfn2ema8A07gmQRYgQn2NBz/KnAUSQIJyGeLyLFPcJkL0QKHr8+dDDoE31uAXObj1++gEOG5F0lrglQGEMj9+brzAibZn68cD0lOlx+JRJ6Hy4tVyowXD3gETmC4RECQLSHwNC6Eq1ZOLBJNA+S7pmbFGLUva1xrY2uuZYEBH3I28YJPz7m9EPJtAnzTuk5oLUvsxLOp0VbAEXEGKDWM71jWtMKAIPLknrLY/Z6twSJiarfUCoOSLlW3EOJKn3UQ3zKZTAxLpGXKyZ3muBAJpkuZwcDVEMPDroRBbBZjvBEZc1Q+nssLc98QFo5OrEWZIgkUqZHbcCGjUDOzFgppnuaiWGm4Z2yc4NhC6YNYJuA5DirXkgBbGxmDo3YB1L2T7+/pGivc5JZYimQMu8rv5aJaGmwsXICW/t+TQwYlzhnTWIrDnAQmfFUpQb4r0yIEcdNleCybGEE3kkBE0nzOoHno+3S5xSBnLkgQoSLgOGxQXKhuRXUOLfQRSo0Fh4x3l0vgQRlFPSaKwYmxpYlpc+M8rlkBU6MFnBbXXOhwt1YigAr0gNdcqAhqBt+yAoJH4Ah6f6b2KkYKRfTAg+8Hi8DkW3AIsQqxQhA8AzdAT+BxvZMSK1wv0bvxPRSB2zqRUaoqAKP7WEfAHPTpxMVg8LBvgDeFxvJ4WheKP995QJLgCaBmqsqE3K+al9OwrEB57ClbmImLvl3ig6eC1ISAyYXAp6ddlTXIlwPUB4kfIGX3tD3rZi5NfJnycXUAZ5eiPI1jMDAp2FlrYDl8HzYR5eZYXo8UQOSVCvdXbuvk3RFJdd9kiL/8blx9V+0WeCShXHrJOSOvCX0fA98iIdpUoNq7VqkejFs8MV9Ym0DRboEVF2A994hGml6PQKO9cKnGt2mFMd+xrkKAJ9ImVKxQ9bFI9Kww6iTmEfD8rqvEQk+6c4b6t7NWX128wByxUAFzgoo8/6P/nuAe1HGluaRQHrfAXFG0pG2A+M4TC0FJy5vLHC17AbnA/wGr+GECy9aXRgAAAABJRU5ErkJggg=="),
            new FormSprite("Diamonds", "magikarpdiamonds", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADo0lEQVR4nO1Zy5EaMRDVUI6AEw5jrw5hjxw3BsIhhj1y3BB8JQzvaVKQq2fV4qnntaRhYMFV7iqK0UhI77X6JxHCf2nLcfcaQwjy+Wclmu+nlki0T/ueTgCsB/rmBDbh/oSGe66xuefkh88P+0rIPLVJRfPMAMaKuT1couO8bMxzE2iMeR4zOl6SVC8QOy6GB0pefNy/9JK4qfY3azR+3L1O7QReSDRByW8eXs+MF8ATeCUBu6AEWxqO3wocxZKQMfiN7z0ixzbBplSzpCxw+PwYPPAq29PZ9ufn998/w9uvP9MYMR+Y7yZZujbBtICA3Z7OAwPOwHskTFbWjDyEOwm1dbV3tXFm99aMjD/YT7hHFJKFC/Co7WQC00e1r9rWb4w0uBPQvpnmB2yIlsRemSSwg0fYipiNiMynJiTjxBzBLONaMkMvGObMHnEEx5QCJMJaAhsb0phTkrLY7VeHh75Bd0Pn1x0YOxJfl5B4PIvltr8xPs/JnB+cfTWBjXfwwJ0g6d/6TtEp4JLNZ3DSxnG6E2F+yFlOIIkb2pINu/U9s3FCirZHkluYkDA80yROlLMlLkycbub06gPovCSRLYqMhxTBzDrT2MEOrBGwYDzw+lsLqkfb2wTM4JnWwmCQ+i4EvEVrucEBkEMoAsbFPTkkUKg4raFS/yz8/rCTpILLywM0T5QgzzomGo158T7C87TGF+CvBHjc5WLQJsAwI9Bx2JDo5JrVuA/hLVy2upb87LwBDkkwZwbtZe+qzWosh3JgtgtaLuszAy6LMxM6QHmB4HWOFnhmQoVTK/jaLggI1ToCR9Dbk7yfZfg8hyXIwOsYa95IINuWaAK12ijwCvC6kAM6g8f13omvaESTvnEfkuOWZYqCLoRFHbKAd0Qs2up8DvhJmPZx9+H3tHKl5THugCWCfoDlhmkXpugBGRN49bEKaZcAvVaxFSSTr60tDzLg3Dndy1yptNAiTsdqf7GuA96VGtNpJ2wRhjtgSUJUwR1xS4+tKRgbYbf/8ENuju31CF5m0bMxno8NAVuQRXsGb2C6ikAGwN4z8PbgT4AXpOz5wVk3rCJh3lGg5GaudQsRkQAqZcF962ICxXtLBrUJ3zUg8cq+ZQQq7/OzvY7pJBHN3OsI6ERsQrILszEeiWs0fVx6f9rzvy7xhZY05wxkF9aArwLr8IUZsEq75tCLzafb6xumtJQUSu065+7/HbAEiH29mo3fRgIA9Wq5F1AX+L90fKlH3HmVTgAAAABJRU5ErkJggg=="),
            new FormSprite("Patches", "magikarppatches", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADo0lEQVR4nO1Zy5EaMRDVUI6AEw5jrw5hjxw3BsIhhj1y3BB8JQzvaVKQq2fV4qnntaRhYMFV7iqK0UhI77X6JxHCf2nLcfcaQwjy+Wclmu+nlki0T/ueTgCsB/rmBDbh/oSGe66xuefkh88P+0rIPLVJRfPMAMaKuT1couO8bMxzE2iMeR4zOl6SVC8QOy6GB0pefNy/9JK4qfY3azR+3L1O7QReSDRByW8eXs+MF8ATeCUBu6AEWxqO3wocxZKQMfiN7z0ixzbBplSzpCxw+PwYPPAq29PZ9ufn998/w9uvP9MYMR+Y7yZZujbBtICA3Z7OAwPOwHskTFbWjDyEOwm1dbV3tXFm99aMjD/YT7hHFJKFC/Co7WQC00e1r9rWb4w0uBPQvpnmB2yIlsRemSSwg0fYipiNiMynJiTjxBzBLONaMkMvGObMHnEEx5QCJMJaAhsb0phTkrLY7VeHh75Bd0Pn1x0YOxJfl5B4PIvltr8xPs/JnB+cfTWBjXfwwJ0g6d/6TtEp4JLNZ3DSxnG6E2F+yFlOIIkb2pINu/U9s3FCirZHkluYkDA80yROlLMlLkycbub06gPovCSRLYqMhxTBzDrT2MEOrBGwYDzw+lsLqkfb2wTM4JnWwmCQ+i4EvEVrucEBkEMoAsbFPTkkUKg4raFS/yz8/rCTpILLywM0T5QgzzomGo158T7C87TGF+CvBHjc5WLQJsAwI9Bx2JDo5JrVuA/hLVy2upb87LwBDkkwZwbtZe+qzWosh3JgtgtaLuszAy6LMxM6QHmB4HWOFnhmQoVTK/jaLggI1ToCR9Dbk7yfZfg8hyXIwOsYa95IINuWaAK12ijwCvC6kAM6g8f13omvaESTvnEfkuOWZYqCLoRFHbKAd0Qs2up8DvhJmPZx9+H3tHKl5THugCWCfoDlhmkXpugBGRN49bEKaZcAvVaxFSSTr60tDzLg3Dndy1yptNAiTsdqf7GuA96VGtNpJ2wRhjtgSUJUwR1xS4+tKRgbYbf/8ENuju31CF5m0bMxno8NAVuQRXsGb2C6ikAGwN4z8PbgT4AXpOz5wVk3rCJh3lGg5GaudQsRkQAqZcF962ICxXtLBrUJ3zUg8cq+ZQQq7/OzvY7pJBHN3OsI6ERsQrILszEeiWs0fVx6f9rzvy7xhZY05wxkF9aArwLr8IUZsEq75tCLzafb6xumtJQUSu065+7/HbAEiH29mo3fRgIA9Wq5F1AX+L90fKlH3HmVTgAAAABJRU5ErkJggg=="),
            new FormSprite("Forehead1", "magikarpforehead1", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADeElEQVR4nO1Zy3HjMAwlNalAp2wZvm4JOeqYGlyOavDRx5SQq8vYnNQCd2ALNAg+kFQkx9qZxYzHjkSBD58HQIxz/6Uu4+tbcM7R55+VoL53LQF4H97bnQiwFujNDejc4w3yj9yje6Ty49eHvkTG7DqlgvqNAIZCuj1dgkFetGbfBlTW7CeNxnuTagWi1wW385yvPfOzVUh6fHx9Q2NCERQ98xQZU6DyO6g0kgbWPByeOYglRsjPNBzip2TIWDdwXQrRBsevDw+6KSyB03BAOtz77z/XtXouOt50r5KSAh4D5DhgekyC78+X5O/T5y824noP6N08Ahp8TCNEQuR5KQSejBBrN8v9bmmnFCnlV1Qa/xADxjqpfAsQ9jh7XaWQn8mtB7v1BoDpUXrU8ppnoFoXG8LgSQi8MMJtZkDJ+5ZhLfdl/pMw+PnbrY1CB8qlbyBodVPJA0XiGIH+fNmmChkvHhE8IKWvkVbrtIxwK7nQtQxj3IgMHUHmeGslYg5MjVwA40rmSakoNi7doPQzVg+Q5KXfNR4VokvpnaUyY/F6oVKSAZRgSg1s3iDR1+LtfgaGgMtiMGO4G8CA9aakCKVICwgJGJVaLQxKO46fpUIz84fXuReUuzrcs9czxVo4OlwelcesihPE7+seN/7cSU8OVJUrPvOydATozxczrabBuXd3SDzmGmQaDhy1GfzdaaDsJkNgkcTc3LgEoihosiLgtLnVradZFxkgHdgCHqVQQmoGX4qCqjQege7PdD0htdOOGCvgWZ9Ob2lAzC3xEpLNMkgkeN7IAA3BnwBXuEvTvWlwM3Hzzp1dQFUHbID6Q8YhJl8JPNIvoy+ehy9AiGRXIiGv16qRqP1JKmogiEMnVXEQLoQXvpERyFrdRvcFsNjuSRcB4lFAG34UOc1r3QLJ+kCi+PPWE1B51WMyP6NSK3qNI1fo1tsJODnWxyPJ0Yk8QkHXhGp5LdFVGzWs9xUzAkrYi+YG+iSCRb600BowEEaAVLnYCJVKy49g0Pm9umYeZvHHGoGVBLE2ihHBzQxIrltHiio1SkBCIX22M6BwPf7W+dxoREBR+LYBrAgpBFHI1lhGfCcK49Lz05b/6wIu1KSq04EorAFfBNbABQs8+rtkxOL0aWZ9JZWWGiXF6iHbS+G/MxbRWz0bfswIAajVy62AmsD/Bdg9OCvgLUsBAAAAAElFTkSuQmCC"),
            new FormSprite("Mask1", "magikarpmask1", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADgElEQVR4nO1Zy3HjMAylPKlAp2wZuW4JOfqYGlSOavBRx5SQq8vYnNwCdyAT9CMIkJQlx9qZxYzHDkUB74H4kIxz/6Uu4+u7d87R558VL753LV7xvvpsdwJgLdCbEzi4xxPqHmnj8Ejlw/enHCIyuw4pL35rAH0h3J4u3khebc6+CVTm7CeMxluTagUi53m385ivvfOzVQg9Pr6+a9uEIih65ykypkDx24swQoI1D/tnbsQSEvi5HN/ip0RkrBNcF0JkYPj+7JRuqpbAy/FN0+E+fv+Z58p90XDVvUpKCngbgNuBoseYQD+dEzKnr19MYn6m6N18BST4GEb3JCGBJxIkgdhmsX9Y2ikhpDLvBe+2kOweQmCsJ1XXAoQ9zl4XIdSF5JYbu/UElN0jetTyWqd5nHQxEQZPQuCBxHZi1fLGDpuVUSynpBvKqhPffvUKiHLZEp9Vo7gqIonjCvTTeZsqZBw8VDCNzzOdFgm3MhcwB8zlDGDM/b2WO7VKJMPpjhCf5SV8oxfYK5pnZPNpMk6ev5I8u2Eyp3nAkgDngvDhbs2xn84zlk5OFEq85lmrWsluDNK1ersPnZrxyI7OEjDcCCiAO4VYVcggg0DAaNySIYAKFSwDToUm5E/ExyFU9HAYKyYbGSRD/RS95oXHrIrj4fds45o/t6Sn8BGVK8uBCLQiGYkUuJvjFD1WUyhWLYCPeRNBC/BRb1cLHyyBvArWTtMCTsa1EBogxokAOrAFvBZCSVIzeDmHKgAbRvAIHEH3E40nSe2kI8YKeNYnw/tFCw84hGR7GU0QPBsyQKvgT0qucJemZ5ejC4mbd+5sgDwvASsGEgDWNpqTrwRe04+rD++rByAtyeZE0rxu5QEL1P4kFCUQLYdOouJouDS86omMQNbqtvYcgMV2T7oIEG8FJPEBYprnugWS9YFE8de1J2jlFb2G74jQil7T+kifduvtRLk5ltcjydUJXqFoY6AaxxJdta2GdVo0V0AIe9E0IG8iWPAQT3NwGyABUuViEiKUll/BaPf3Ysy8zMKTWMOpzsPcKMYKbkYgGbeuFBccHX0hfLYjUBiPv41zb42E11bhbgKsSFOorEI2xyJxzyqMS+9PW/6vq+RCTao6nbIKa8AXgTXkggVe+7tEYnH4NGd9JZSWkkKxesj2UvjvjJXorZ71P0YCALV6uRVQE/i/FqtCoP335dwAAAAASUVORK5CYII="),
            new FormSprite("Forehead2", "magikarpforehead2", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAC7UlEQVR4nO1YwXHcMAykblyBX04Z/qYEP/NMDS6HNeTpp0vI12XYr2sBGTKCDYELEJR0d8pMMMO5k0gCuyBIQEzpv/QlPzxRSqm0f1ZI/R5aCHgf9h1OBFgL9O4ETunyhKZL2jhdUvnzx6t+VcgcOqRI/UcAyQm3mwsZmxeNOTaBzpjjhFH+SlJRIHocpRvKp/Hzj8coiV29f9ri8fzwVJ9n8IVEF1SZc/N65vwFuIJnEmIVmGDPw3RV4FI0CUVgAdIikvsEt4llQIMW4CtgQ4cmsgt4L83XMqAAu395q+MEyEZ4jJzLDwX488frpPouWmbAWOd45xhXnm/mSwHe32UF0ClUQC3Ac8f9y1upb4rXaivPK0+a3Tw/KcP08/s7HDiDRYZhKLDHC2EVQt25IzIh72v59fsbex6KAbCCc8B/jhlC7IHQse0cjQiI+a5zXNJ61LYB92iM1DNaZ6A63SRbPGj1d0ntSQJVlNGqMZxltxDIy6qX9iJhGpZggXGvRe+UKFLXewojG3tNwiILeHnHjfvQ0cdS+zrHoAeiyTHBS4AJlSJzP5c2CPtfw5q5YKwJNmLNDRyjhL6ReS6oCrA+vTyAAJwcmOMJKSKL+WHwKGalImsV9GZFwLkI1C2B/bYWPAQGThI4FvQvQI/YywZ4UQm7Ur2CgHsEZF/QkAteitgDUCesIvWpIxXPffqUYZILXaWyVR86PNbVn+dnNb+ZZ0nkG9YLi8a76qMHrl5WJ46hOyQoZBoCndiGRLyMmlfeYNx5SWUOJ/iFJZdazgGhNanQ64LaLCBU9OYeWWYUPjBxnZ1Lg6ErGAeoRyBiAIURSZvGRcH4SiES6p1ulg53TDL2wMDX4CoCi/eajJGMPCCESAA72wg475s4Nn7pKgQyzr6aACtvxlgk1qxCHr0/jdxfgr3Qk67OBAhsAe8CC+yFBpjzLGWRldeET3hSJ5RGSY2u1j5i1fRGvoDJK2G5HglhJOrlKKBQNPwBpKb/VIoNc8QAAAAASUVORK5CYII="),
            new FormSprite("Mask2", "magikarpmask2", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADN0lEQVR4nO1YQXLbMAykNHlBTukzcu0Tcuyxb/Bz9IYec8wTevUzklO+wA5VQgbBBQhGtK3OFDMe2yIE7IIAASmE/9KW5eklhhDS55+VKL4PLRFEH64dThhYDfRwAnO4PqHpmj7maxo/fbzJS4nMoVMqit8IYDTS7e4SleJFOscm0NA5ThotlyblBSL1YrijbM4/fzx7SQyN/rwn4svTy/o/g08kmqDSPXefZz4vgFfwRILtAhFsRTjeFDgXSUIQKEBqRJY2wX2iOZCgGfgVsGJDEhkC3mrz6xiQgD2+nlc9BhIK6dG9dD0BP328ybUhY8bsAc8j/Ov3twR0/XglgWfRl+PEcAIJbJHjtJBA50iuH0mCdB0nzbABb+J/UpR+fn+HihnspBFmeqsORZyiz1KouDfsJDM3psdAaWM4mvJ6kLYa4IfIRoAihnJbI4bWZaGL/Jcy7a2HlQCLEsxtIE2nvA4aJIYInNt5p7Vu5p1Y6wek1+G/uwbUYSwXtuocFf7omWcpp94oCfAi285rnkrKoFacQC3nuVYkkMj8artWpDivHdkt6e+kAUwnDukh8KIbS3seiUmf4Skw0m9ak4Yrp1ZvQJIJVOe7p4hPF8DVKJLXaTrY8D2g3JXHZo76ZKVMAv7X0XlzLiKm7UJkvwvwdH8KII00LEChIuAZAR5fz1XnJeAEkkesZTALgdrSR0Rcgod2qwpPhsiY9hqER5nrc6GjWH4CeLzkNsA8ZqZhBUyC03TBegG6x9+igPf0IzIKgVsE+JrTkQmeC3vmhjarXELDFyhGmYdwjKbiEy7ME+rExg5xv3ty9TzDWmlRRVcMeHD3lrwDxlOfe9xAKVMRaOQ2JOJ8V6oJXK/6AEnqBdQN0fGKJkzZOfm2s9RrgtotIFVkcfdsM0ofOQOF1kuDrnHcAGoR8DhAaRS5TzGS99hukxDX0ESJbJg6QakB40XZEALFdUlGaUYWkIhIAD/7CBjXqzxWvuNNCCy4+0oCZLzS0Uh8ZReW3mdpz/tLUAstadoMgMAe8CYwRy1UwIz/XIqu/JX0cd/USKVeUr27NUa0mV7pF7B5BSy3I8GceKPsBeTKhj9m6QEbpxvFLQAAAABJRU5ErkJggg=="),
            new FormSprite("Saucy", "magikarpsaucy", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADU0lEQVR4nO1Zy5HbMAylNFuBT5syck0Je9xjanA5qiFHH13CXl3G+qQWmIEk2OATAEKW/MlMMOPxSiKJ94hH8smb0v+oR/f+kVNK9PlnI8P3S0dWZl999nIhwFqgNyfQpvsTau6Zo73n4PvzEW8RmZeWVIa/NYDZkdvTIxuLV2vz2gQqbV5HRt31kIoCwXY5PTEuyfvPn1ESm85+u2bGu/eP4XoCTySqoKjP0/1MfwU8gGcSogpMsDbD+aHAZSAJSQCvLSJdneA6CVGC/fnY4GkqwdcSkGR+//pOChEee1V4Aww2gEDuDqcmCphidzjxehjiz9ePgcQ0DtuLTWzGWwS8BE5g2CJIkF4QeOqX0kmzE6tCmwHSrjmz0EdtyzOu3eNqblWBVl5Mu0kU/HA93VdnfJx1ndBWlWgr7vECwJmthoHiWExEgo+uo8UEeIfQZtQiFnl+1f8YsKaatVVole1SlQWEm5RA0vbJepcktI1hizB9e+TAgZN41t84saWc8hZrwDRjU3VMf88aFztM4XnovnwmwffBSoDrVftoD2vW10zOVeOZh/G9TxHcV7MsasOa3Y34fzR1iry8yJCnkCB/+BlujTJJAwt8SQznCUiqSmA/7maDXNGKTM/ZHWjYx8ToPgVjJFgESKXoWwGfoaKXxc59vUX/tvBlY2bCaFCaod2BZm+ccQYcrVw/VovBS89V+DEwgzMCJBUJ9AJk2GW+dClxIg84k8TYn4+FIZQTGAGPUZQZS1/7jRNlIiVg5evFroJyQ9kIiboxe4Pivz0C8lkwkbe7FCHWgDrmrBzariMHFodaYaundsVYYJ9l28sL0m60HcX4Urq32O/IO6wpC606lsXACittZYTthiaZIpkkKRPiAab0NT1Xd+MvGOobGW+XJAnaKXB34DaYVBxEOJZ3nXisyrYblpAmFVzcS8qsyac4uNIUnt1Y9BOMA9QjEEmgySjLnIZvioP3SMA9U+8MJGKBk7EGTNe5EYHiPpIxDqOaH0qGTLYj4Nyf6dj4zg8h0OmnLxJInny071uq0C39/TTyf11lLdSiOmZSCKwB7wILrIUZMOdaRnEq3yKfcKeKlJaSWlqtbUKZJe0AlM8iayE9lIRIEp3lKKCQGv4C0PlOO5kjzz0AAAAASUVORK5CYII="),
            new FormSprite("Raindrop", "magikarpraindrop", "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAADJElEQVR4nO1YwXHjMAyEPFeBX7ky8r0S/Lxnakg5rOGefqaEfFPG+aUWmIFGUEAQACGLinUzhxmNLYkEdwEQAAXwX9qSni4ZAPD6ZyWL30NLVqyvvjucMLAW6O4ETrA/oWHPNU57Kn+9vclHSObQIZXFfw1gdsLt4ZKNzauNOTaBxpjjhFH6KlJRIHJchgfKsvj4+zlKoqv1T1ssnp4u0/0MHkk0QeGch/cz4xfgCTyRYF4ggi0L5909QMBfb28DL0pEAuXl199iDnoCr/m5SSR18IZLABfmwJmV4Xz9GPDi4+U9gTSIkO5N4ilAsNACGJn75/3n4qXz9YPW7dJmnNaCt9ImDyk+HgXBI4l53KR/K3CPQAFehAoPqcEiQbHteGyfBi/5WcNtDfj+IF2kT2Qp7q3cEb6tsJUOjfdLiqUHConcAXMBwu0gHTEJSBIo3Umk2oJb76uwsUhAJy+4Vo+GkMhEuTVu7EhCC59o16jGuGZxbT+MwTASIZ57kahASgIMePSKflPKkU3sKYxs7OmSKTYwDzTglJr5O1lQ+ISl8t7Rs0zFkBeyQGcK80eAqs2guYgDDcHakXphyZwXJM/qIlSKuYECmfkZmRdAcd6ocPxY2d5W1kGl2Oecr2i9ol+aLNZSKPROhyTu+dniy69cf2iFD2/GZkCVewmoBZxIOiEDHDxKBLwUdcPwe2us8n4JKe/zyqj0Tlat0Cq6RUIF7hGQ+X3tUTI5e4XtAVWndoKqsg5XrIURS32FLgw9paWuwiAJ/Tx0eSsfbcO9M2wrG6m5W7YYMnRAyTiG7pCorYAkoPU/8quEMlcratlaL0JAcwm5asoKmClkdqAxclGRVbgu7x5IVyPthkNICxW5ude4Wdv8ReGCWbx2I5KBPAIaAA+oJVo2yXxNo2+Kg/dIiGdmvBOQSAsMxh5Y02rfQ6B4LskYxajVD4F1tu5GwHlexbHxm7+FQNKrryQAXvhov/d4Ia3ZvAZAb0w0Rps6QSGwBbwLLLAXKmDOPZeiKt8TPuFJjVBaS2qtt/qIYiWtAPJ3kb0A30qCLRK1chRQKBo+Afb6NZg82Q1FAAAAAElFTkSuQmCC")
        };

        private readonly Brush bg = BrushFrom("#0D1117");
        private readonly Brush shell = BrushFrom("#161B22");
        private readonly Brush title = BrushFrom("#0B0F14");
        private readonly Brush card = BrushFrom("#21262D");
        private readonly Brush cardHover = BrushFrom("#2A313A");
        private readonly Brush cardChecked = BrushFrom("#203A5C");
        private readonly Brush border = BrushFrom("#30363D");
        private readonly Brush text = BrushFrom("#F0F6FC");
        private readonly Brush muted = BrushFrom("#8B949E");
        private readonly Brush accent = BrushFrom("#58A6FF");
        private readonly Brush danger = BrushFrom("#DA3633");

        public MainWindow()
        {
            Title = "Magikarp Forms Checklist";
            Width = 1040;
            Height = 820;
            MinWidth = 980;
            MinHeight = 780;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.CanResize;
            Background = bg;

            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(7),
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            });

            BuildUi();
            LoadState();
            SetPreview(Sprites[0], "Hover or click a form to preview it here.");
            UpdateCounter();

            Closing += delegate
            {
                SaveState();
            };
        }

        private void BuildUi()
        {
            var root = new Grid();
            root.Background = bg;
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(44) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Content = root;

            var titleBar = new Grid();
            titleBar.Background = title;
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            titleBar.MouseLeftButtonDown += TitleBarMouseLeftButtonDown;
            Grid.SetRow(titleBar, 0);
            root.Children.Add(titleBar);

            var titleText = new TextBlock();
            titleText.Text = "Magikarp Forms Checklist";
            titleText.Foreground = text;
            titleText.FontFamily = new FontFamily("Segoe UI");
            titleText.FontSize = 14;
            titleText.VerticalAlignment = VerticalAlignment.Center;
            titleText.Margin = new Thickness(18, 0, 0, 1);
            Grid.SetColumn(titleText, 0);
            titleBar.Children.Add(titleText);

            var minButton = MakeWindowButton("─", false);
            var maxButton = MakeWindowButton("□", false);
            var closeButton = MakeWindowButton("×", true);

            minButton.Click += delegate { WindowState = WindowState.Minimized; };
            maxButton.Click += delegate
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            };
            closeButton.Click += delegate
            {
                SaveState();
                Close();
            };

            Grid.SetColumn(minButton, 1);
            Grid.SetColumn(maxButton, 2);
            Grid.SetColumn(closeButton, 3);
            titleBar.Children.Add(minButton);
            titleBar.Children.Add(maxButton);
            titleBar.Children.Add(closeButton);

            var shellBorder = new Border();
            shellBorder.Background = shell;
            shellBorder.BorderBrush = border;
            shellBorder.BorderThickness = new Thickness(1);
            shellBorder.CornerRadius = new CornerRadius(18);
            shellBorder.Margin = new Thickness(16);
            shellBorder.Padding = new Thickness(22);
            Grid.SetRow(shellBorder, 1);
            root.Children.Add(shellBorder);

            var main = new Grid();
            main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(78) });
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            shellBorder.Child = main;

            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(112) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(94) });
            header.Margin = new Thickness(0, 0, 0, 10);
            Grid.SetColumnSpan(header, 2);
            Grid.SetRow(header, 0);
            main.Children.Add(header);

            var headingStack = new StackPanel();
            headingStack.Orientation = Orientation.Vertical;
            headingStack.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(headingStack, 0);
            header.Children.Add(headingStack);

            var h1 = new TextBlock();
            h1.Text = "Magikarp Forms";
            h1.Foreground = text;
            h1.FontFamily = new FontFamily("Segoe UI");
            h1.FontSize = 25;
            h1.FontWeight = FontWeights.SemiBold;
            headingStack.Children.Add(h1);

            var sub = new TextBlock();
            sub.Text = "Offline native checklist with zoom preview and automatic saving.";
            sub.Foreground = muted;
            sub.FontFamily = new FontFamily("Segoe UI");
            sub.FontSize = 13;
            sub.Margin = new Thickness(1, 5, 0, 0);
            headingStack.Children.Add(sub);

            counterText.Foreground = text;
            counterText.FontFamily = new FontFamily("Segoe UI");
            counterText.FontSize = 13;
            counterText.VerticalAlignment = VerticalAlignment.Center;
            counterText.HorizontalAlignment = HorizontalAlignment.Center;

            var counterPill = new Border();
            counterPill.Background = card;
            counterPill.BorderBrush = border;
            counterPill.BorderThickness = new Thickness(1);
            counterPill.CornerRadius = new CornerRadius(16);
            counterPill.Height = 34;
            counterPill.Margin = new Thickness(0, 20, 10, 0);
            counterPill.Child = counterText;
            Grid.SetColumn(counterPill, 1);
            header.Children.Add(counterPill);

            var reset = MakeButton("Reset");
            reset.Margin = new Thickness(0, 20, 0, 0);
            reset.Click += delegate
            {
                foreach (var pair in checkboxes)
                {
                    pair.Value.IsChecked = false;
                }

                SaveState();
                UpdateCounter();
            };
            Grid.SetColumn(reset, 2);
            header.Children.Add(reset);

            var grid = new UniformGrid();
            grid.Columns = 4;
            grid.Rows = 5;
            grid.Margin = new Thickness(0, 0, 18, 0);
            Grid.SetRow(grid, 1);
            Grid.SetColumn(grid, 0);
            main.Children.Add(grid);

            foreach (var sprite in Sprites)
            {
                var cardControl = MakeCard(sprite);
                grid.Children.Add(cardControl);
            }

            var preview = MakePreviewPanel();
            Grid.SetRow(preview, 1);
            Grid.SetColumn(preview, 1);
            main.Children.Add(preview);
        }

        private Button MakeWindowButton(string glyph, bool isClose)
        {
            var button = new Button();
            button.Content = glyph;
            button.FontFamily = new FontFamily("Segoe UI");
            button.FontSize = 15;
            button.Foreground = text;
            button.Background = title;
            button.BorderThickness = new Thickness(0);
            button.Cursor = Cursors.Hand;
            button.Focusable = false;

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.BackgroundProperty, title));
            style.Setters.Add(new Setter(Button.ForegroundProperty, text));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));

            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "border";
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));

            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);
            template.VisualTree = borderFactory;

            var hover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Button.BackgroundProperty, isClose ? danger : BrushFrom("#202734")));

            var pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Button.BackgroundProperty, isClose ? BrushFrom("#B42324") : BrushFrom("#2D3545")));

            template.Triggers.Add(hover);
            template.Triggers.Add(pressed);
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            button.Style = style;
            WindowChrome.SetIsHitTestVisibleInChrome(button, true);

            return button;
        }

        private Button MakeButton(string label)
        {
            var button = new Button();
            button.Content = label;
            button.Height = 34;
            button.FontFamily = new FontFamily("Segoe UI");
            button.FontSize = 13;
            button.Foreground = text;
            button.Background = card;
            button.BorderBrush = border;
            button.BorderThickness = new Thickness(1);
            button.Cursor = Cursors.Hand;
            button.Focusable = false;
            return button;
        }

        private Border MakeCard(FormSprite sprite)
        {
            var outer = new Border();
            outer.Background = card;
            outer.BorderBrush = border;
            outer.BorderThickness = new Thickness(1);
            outer.CornerRadius = new CornerRadius(15);
            outer.Margin = new Thickness(6);
            outer.Padding = new Thickness(10);
            outer.Cursor = Cursors.Hand;

            var stack = new StackPanel();
            stack.VerticalAlignment = VerticalAlignment.Center;
            outer.Child = stack;

            var check = new CheckBox();
            check.HorizontalAlignment = HorizontalAlignment.Center;
            check.Margin = new Thickness(0, 0, 0, 8);
            check.Cursor = Cursors.Hand;
            checkboxes[sprite.Slug] = check;
            check.Checked += delegate
            {
                outer.Background = cardChecked;
                outer.BorderBrush = accent;
                SaveState();
                UpdateCounter();
                SetPreview(sprite, "Selected");
            };
            check.Unchecked += delegate
            {
                outer.Background = card;
                outer.BorderBrush = border;
                SaveState();
                UpdateCounter();
                SetPreview(sprite, "Updated");
            };
            stack.Children.Add(check);

            var imageTile = new Border();
            imageTile.Width = 92;
            imageTile.Height = 92;
            imageTile.Background = Brushes.White;
            imageTile.BorderBrush = BrushFrom("#D8DEE8");
            imageTile.BorderThickness = new Thickness(1);
            imageTile.CornerRadius = new CornerRadius(14);
            imageTile.HorizontalAlignment = HorizontalAlignment.Center;
            imageTile.Padding = new Thickness(10);
            stack.Children.Add(imageTile);

            var img = new Image();
            img.Source = ImageFromBase64(sprite.Base64);
            img.Stretch = Stretch.Uniform;
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            imageTile.Child = img;

            var name = new TextBlock();
            name.Text = sprite.Name;
            name.Foreground = text;
            name.FontFamily = new FontFamily("Segoe UI");
            name.FontSize = 13;
            name.FontWeight = FontWeights.SemiBold;
            name.TextAlignment = TextAlignment.Center;
            name.Margin = new Thickness(0, 9, 0, 0);
            stack.Children.Add(name);

            outer.MouseEnter += delegate
            {
                if (check.IsChecked != true)
                {
                    outer.Background = cardHover;
                }

                SetPreview(sprite, "Preview");
            };

            outer.MouseLeave += delegate
            {
                if (check.IsChecked == true)
                {
                    outer.Background = cardChecked;
                }
                else
                {
                    outer.Background = card;
                }
            };

            outer.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.OriginalSource is CheckBox)
                {
                    return;
                }

                check.IsChecked = check.IsChecked != true;
                SetPreview(sprite, check.IsChecked == true ? "Selected" : "Preview");
            };

            return outer;
        }

        private Border MakePreviewPanel()
        {
            var panel = new Border();
            panel.Background = BrushFrom("#0F1520");
            panel.BorderBrush = border;
            panel.BorderThickness = new Thickness(1);
            panel.CornerRadius = new CornerRadius(18);
            panel.Padding = new Thickness(18);

            var stack = new StackPanel();
            panel.Child = stack;

            var label = new TextBlock();
            label.Text = "Zoom Preview";
            label.Foreground = text;
            label.FontFamily = new FontFamily("Segoe UI");
            label.FontSize = 18;
            label.FontWeight = FontWeights.SemiBold;
            label.Margin = new Thickness(0, 0, 0, 6);
            stack.Children.Add(label);

            previewHint.Foreground = muted;
            previewHint.FontFamily = new FontFamily("Segoe UI");
            previewHint.FontSize = 12;
            previewHint.TextWrapping = TextWrapping.Wrap;
            previewHint.Margin = new Thickness(0, 0, 0, 18);
            stack.Children.Add(previewHint);

            var whiteStage = new Border();
            whiteStage.Width = 236;
            whiteStage.Height = 236;
            whiteStage.Background = Brushes.White;
            whiteStage.BorderBrush = BrushFrom("#D8DEE8");
            whiteStage.BorderThickness = new Thickness(1);
            whiteStage.CornerRadius = new CornerRadius(18);
            whiteStage.HorizontalAlignment = HorizontalAlignment.Center;
            whiteStage.Padding = new Thickness(18);
            stack.Children.Add(whiteStage);

            previewImage.Stretch = Stretch.Uniform;
            RenderOptions.SetBitmapScalingMode(previewImage, BitmapScalingMode.NearestNeighbor);
            whiteStage.Child = previewImage;

            previewName.Foreground = text;
            previewName.FontFamily = new FontFamily("Segoe UI");
            previewName.FontSize = 24;
            previewName.FontWeight = FontWeights.SemiBold;
            previewName.TextAlignment = TextAlignment.Center;
            previewName.Margin = new Thickness(0, 20, 0, 0);
            stack.Children.Add(previewName);

            var note = new TextBlock();
            note.Text = "Sprites are shown on white because several forms rely on contrast against a light background.";
            note.Foreground = muted;
            note.FontFamily = new FontFamily("Segoe UI");
            note.FontSize = 12;
            note.TextWrapping = TextWrapping.Wrap;
            note.TextAlignment = TextAlignment.Center;
            note.Margin = new Thickness(0, 14, 0, 0);
            stack.Children.Add(note);

            return panel;
        }

        private void SetPreview(FormSprite sprite, string hint)
        {
            previewImage.Source = ImageFromBase64(sprite.Base64);
            previewName.Text = sprite.Name;
            previewHint.Text = hint;
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                return;
            }

            try
            {
                DragMove();
            }
            catch
            {
            }
        }

        private void LoadState()
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(SavePath))
            {
                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length != 2)
                {
                    continue;
                }

                string slug = parts[0].Trim();
                string value = parts[1].Trim();

                if (checkboxes.ContainsKey(slug))
                {
                    checkboxes[slug].IsChecked = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private void SaveState()
        {
            try
            {
                Directory.CreateDirectory(saveDir);

                var lines = new List<string>();

                foreach (var pair in checkboxes)
                {
                    lines.Add(pair.Key + "=" + (pair.Value.IsChecked == true ? "1" : "0"));
                }

                File.WriteAllLines(SavePath, lines.ToArray());
            }
            catch
            {
            }
        }

        private void UpdateCounter()
        {
            int checkedCount = 0;

            foreach (var pair in checkboxes)
            {
                if (pair.Value.IsChecked == true)
                {
                    checkedCount++;
                }
            }

            counterText.Text = checkedCount + "/" + checkboxes.Count + " checked";
        }

        private static ImageSource ImageFromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            using (var stream = new MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        private static SolidColorBrush BrushFrom(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }
}